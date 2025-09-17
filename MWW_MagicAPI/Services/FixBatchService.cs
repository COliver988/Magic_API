using CsvHelper;
using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Shopfloor;
using System.Text;

namespace MWW_MagicAPI.Services;

public class FixBatchService : IFixBatchService
{
    private readonly IShopfloorDbContextFactory _contextFactory;
    private readonly ExentaDbContext _exentaContext;
    private readonly MagicDbContext _magicDbContext;
    private ShopfloorDbContext _context;

    private record WorkOrderData
    {
        public int ProdStageNo { get; set; }
        public int ProdNoCompany { get; set; }
        public int OpenSeq { get; set; }
        public int ItemNo { get; set; }
        public string Style { get; set; }
        public string StyleName { get; set; }
        public string Label { get; set; }
        public string Color { get; set; }
        public string ColorDesc { get; set; }
        public string Dimension { get; set; }
        public string DimensionDesc { get; set; }
        public string Size { get; set; }
        public string SizeDesc { get; set; }
        public int? ProdLineQty { get; set; }
        public string UOM { get; set; }
        public string DetailShipDate { get; set; }
        public string DtlDueDate { get; set; }
        public int OrderNoCompany { get; set; }
        public string PONumber { get; set; }
        public string Warehouse { get; set; }
        public string Consolidate { get; set; }
        public string Message { get; set; }
    }

    private string[] _workOrderHeaderUnits = {
            "Workorder", "Batch", "Seq", "Quantity", "Unit", "Thumbnail", "Content", "Flag"
        };

    private record MagicUnit
    {
        public int ProdNoCompany { get; set; }
        public int? OpenSeq { get; set; }
        public string BatchID { get; set; }
        public int PrintOrder { get; set; }
    }

    public FixBatchService(IShopfloorDbContextFactory contextFactory, ExentaDbContext exentaContext, MagicDbContext magicDbContext)
    {
        _contextFactory = contextFactory;
        _exentaContext = exentaContext;
        _magicDbContext = magicDbContext;
    }

    public async Task<List<Unit>> GetMissingBatches(string batchId)
    {
        _context = _contextFactory.GetContext(batchId);
        List<MagicUnit> missingUnits = await getMissingBatches(batchId);
        if (!missingUnits.Any()) return null;
        List<WorkOrderData> workorderData = new List<WorkOrderData>();
        foreach (MagicUnit unit in missingUnits)
            workorderData.Add(await batchUnitValues(unit.ProdNoCompany, unit.OpenSeq.Value));
        write_to_workorder_file(workorderData);
        write_to_workorder_units_file(batchId);

        return null;
    }

    /// <summary>
    /// write to file; 1st to temp file, then move to final location to prevent hot folder grabbing incomplete file
    /// </summary>
    /// <param name="workorderFileData"></param>
    private void write_to_workorder_file(List<WorkOrderData> workorderData)
    {
        string tempFilePath = Path.Combine(Path.GetTempFileName());
        using (StreamWriter writer = new StreamWriter(tempFilePath, false, Encoding.UTF8))
        using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(workorderData);
        }

        // mive to final location and cleanup
        if (File.Exists(tempFilePath))
        {
            string finalFilePath = Path.Combine(AppContext.BaseDirectory, "Import", "Workorders.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(finalFilePath)!);
            if (File.Exists(finalFilePath))
                File.Delete(finalFilePath);
            File.Move(tempFilePath, finalFilePath);
            File.Delete(tempFilePath);
        }
    }

    private async void write_to_workorder_units_file(string batchId)
    {
        string headerLine = string.Join(",", _workOrderHeaderUnits);
        string filePath = Path.Combine(AppContext.BaseDirectory, "Import", $"WorkorderUnits_{batchId}.csv");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        List<string> unitData  = await GetFormattedPrintDetails(batchId); 

        using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
            writer.WriteLine(headerLine);
            foreach (string unitDataLine in unitData)
                writer.WriteLine(unitDataLine);
        }
    }

    public async Task<List<string>> GetFormattedPrintDetails(string batchId)
    {
        var result = (from dpd in _magicDbContext.DyePrintDetails
                      join ack in _magicDbContext.ExentaPOLinesWithAckNos
                          on new { dpd.PO, LineNo = (int)dpd.Ln_No } equals new { ack.PO, LineNo = ack.LN_NO }
                      where dpd.BatchID == batchId
                            && !new[] { "queue", "ready", "pods" }.Contains(dpd.Status)
                      orderby dpd.printedOrder
                      select new
                      {
                          Formatted = ack.ProdNoCompany + "-" + ack.OpenSeq.ToString() + "," +
                                      dpd.BatchID + "," +
                                      dpd.printedOrder.ToString() + ",1," +
                                      dpd.BatchID + "_" + dpd.printedOrder.ToString() + "," +
                                      dpd.PO + "_" + dpd.printedOrder.ToString() + ".jpg,,I"
                      }).ToList();

        return result.Select(r => r.Formatted).ToList();

    }

    private async Task<List<MagicUnit>> getMissingBatches(string batchId)
    {
        List<Unit> unitsInShopfloor = await getUnitsFromShopfloor(batchId);
        List<MagicUnit> unitsInMagic = await getUnitsFromMagic(batchId);
        var shopFloorKeys = unitsInShopfloor
            .Select(u => $"{u.BatchId}_{u.BatchSeq}")
            .ToHashSet();

        var filteredMagicUnits = unitsInMagic
            .Where(d => !shopFloorKeys.Contains($"{d.BatchID}_{d.PrintOrder}"))
            .OrderBy(d => d.PrintOrder)
            .ToList();

        //testing!
        filteredMagicUnits = unitsInMagic;
        return filteredMagicUnits;
    }

    private Task<List<Unit>> getUnitsFromShopfloor(string batchId)
    {
        var units = _context.Units
            .Where(u => u.BatchId == batchId)
            .AsNoTracking()
            .ToListAsync();
        return units;
    }

    private async Task<List<MagicUnit>> getUnitsFromMagic(string batchId)
    {
        var rawResults = await _magicDbContext.DyePrintDetails
            .Where(dpd => dpd.BatchID == batchId)
            .Join(
                _magicDbContext.ExentaPOLinesWithAckNos,
                dpd => new { dpd.PO, LineNo = (int)dpd.Ln_No },
                ack => new { PO = ack.PO, LineNo = ack.LN_NO },
                (dpd, ack) => new { dpd, ack }
            )
            .AsNoTracking()
            .ToListAsync();

        var magicUnits = rawResults
            .Select(x => new MagicUnit
            {
                ProdNoCompany = int.TryParse(x.ack.ProdNoCompany?.Trim(), out var prodNo) ? prodNo : 0,
                OpenSeq = x.ack.OpenSeq,
                BatchID = x.dpd.BatchID!,
                PrintOrder = x.dpd.printedOrder.HasValue ? (short)x.dpd.printedOrder.Value : x.dpd.PrintOrder
            })
            .ToList();

        return magicUnits;
    }

    private async Task<WorkOrderData> batchUnitValues(int prodNoCompany, int openSeq)
    {
        List<Dictionary<string, string>> exentaData = await getExentaOrderDataAsync(prodNoCompany);
        string consolidate = goesToConsolidation(exentaData);
        return  await getExentaUnitDataAsync(prodNoCompany, openSeq, consolidate);
    }

    private string goesToConsolidation(List<Dictionary<string, string>> exentaData)
    {
        if (exentaData.Count > 1 ||
            exentaData.Any(d => d.TryGetValue("orderorigin", out var origin) && origin.Contains("STS")) ||
            exentaData.Any(d => d.TryGetValue("prodlineqty", out var qtyStr) &&
                                     double.TryParse(qtyStr, out var qty) && qty > 1.0))
        {
            return "Y";
        }
        else
        {
            return "N";
        }
    }

    private async Task<WorkOrderData> getExentaUnitDataAsync(int prodNoCompany, int sequence, string consolidate)
    {
        // Step 1: Get ProdOrderHeader
        var poh = await _exentaContext.ProdOrderHeaders
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.PRODNOCOMPANY == prodNoCompany);

        if (poh == null) return null;

        // Step 2: Get ProdOrderDetail
        var pod = await _exentaContext.ProdOrderDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PRODSTAGENO == 1 && p.OPENSEQ == sequence && p.PRODNO == poh.PRODNO);

        if (pod == null) return null;


        // Step 3: Get PickOrderHeader
        var pioh = await _exentaContext.PickOrderHeaders
            .AsNoTracking()
            .FirstOrDefaultAsync(ph => ph.ORDERNO == poh.ORDERNO);

        // Step 4: Get PickOrderDetail
        var piod = pioh != null
            ? await _exentaContext.PickOrderDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(pd => pd.PICKNO == pioh.PICKNO && pd.SEQUENCE == pod.ORDERSEQ)
            : null;

        // Step 5: Get StyleItem
        var si = await _exentaContext.StyleItems
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ITEMNO == pod.ITEMNO);

        if (si == null) return null;

        // Step 6: Get Style
        var st = await _exentaContext.Styles
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.STYLE == si.STYLE);

        // Step 7: Get Size (optional)
        var s = !string.IsNullOrEmpty(si.SIZE)
            ? await _exentaContext.Sizes.AsNoTracking().FirstOrDefaultAsync(sz => sz.SIZE == si.SIZE)
            : null;

        // Step 8: Get Dimension (optional)
        var d = !string.IsNullOrEmpty(si.DIMENSION)
            ? await _exentaContext.Dimensions.AsNoTracking().FirstOrDefaultAsync(dim => dim.DIMENSION == si.DIMENSION)
            : null;

        // Step 9: Get Color (optional)
        var c = !string.IsNullOrEmpty(si.COLOR)
            ? await _exentaContext.Colors.AsNoTracking().FirstOrDefaultAsync(col => col.COLOR == si.COLOR)
            : null;

        // Step 10: Project the result
        return new WorkOrderData
        {
            ProdStageNo = pod.PRODSTAGENO,
            ProdNoCompany = poh.PRODNOCOMPANY,
            OpenSeq = pod.OPENSEQ,
            ItemNo = pod.ITEMNO,
            Style = si.STYLE,
            StyleName = st?.STYLENAME,
            Label = si.LABEL,
            Color = si.COLOR,
            ColorDesc = c?.COLORDESC,
            Dimension = si.DIMENSION,
            DimensionDesc = d?.DIMENSIONDESC,
            Size = si.SIZE,
            SizeDesc = s?.SIZEDESC,
            ProdLineQty = (int?)pod.PRODLINEQTY,
            UOM = pod.UOM,
            DetailShipDate = pod.SHIPDATE.ToString("MM/dd/yyyy"),
            DtlDueDate = pod.DUEDATE.ToString("MM/dd/yyyy"),
            OrderNoCompany = poh.ORDERNOCOMPANY,
            PONumber = piod?.WEBUDF03,
            Warehouse = poh.WAREHOUSE,
            Consolidate = consolidate,
            Message = ""
        };
    }

    private string ToCsv(dynamic item)
    {
        var values = new[]
        {
        item.PRODSTAGENO?.ToString(),
        item.PRODNOCOMPANY?.ToString(),
        item.OPENSEQ?.ToString(),
        item.ITEMNO?.ToString(),
        item.STYLE?.ToString(),
        item.STYLENAME?.ToString(),
        item.LABEL?.ToString(),
        item.COLOR?.ToString(),
        item.COLORDESC?.ToString(),
        item.DIMENSION?.ToString(),
        item.DIMENSIONDESC?.ToString(),
        item.SIZE?.ToString(),
        item.SIZEDESC?.ToString(),
        item.PRODLINEQTY?.ToString(),
        item.UOM?.ToString(),
        item.DETAILSHIPDATE?.ToString(),
        item.DTLDUEDATE?.ToString(),
        item.ORDERNOCOMPANY?.ToString(),
        item.WEBUDF03?.ToString(),
        item.WAREHOUSE?.ToString(),
        item.Consolidate?.ToString(),
        item.Message?.ToString()
    };

        // Escape values that contain commas, quotes, or newlines
        var escaped = values.Select(v =>
            string.IsNullOrEmpty(v) ? "" :
            v.Contains(",") || v.Contains("\"") || v.Contains("\n")
                ? $"\"{v.Replace("\"", "\"\"")}\""
                : v
        );

        return string.Join(",", escaped);
    }

    private async Task<List<Dictionary<string, string>>> getExentaOrderDataAsync(int prodNoCompany)
    {
        // Optional: Begin a transaction with ReadUncommitted isolation
        using var transaction = await _exentaContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var query = from poh in _exentaContext.ProdOrderHeaders
                    join pod in _exentaContext.ProdOrderDetails
                        on poh.PKEY equals pod.FKEY
                    join oh in _exentaContext.OrderHeaders
                        on poh.ORDERNO equals oh.ORDERNO
                    where pod.PRODSTAGE == "MAKE" && poh.PRODNOCOMPANY == prodNoCompany
                    select new
                    {
                        oh.ORDERORIGIN,
                        pod.PRODLINEQTY
                    };

        var result = await query
            .Select(e => new Dictionary<string, string>
            {
            { "orderorigin", e.ORDERORIGIN.Trim() ?? "" },
            { "prodlineqty", e.PRODLINEQTY.ToString().Trim() ?? "" }
            })
            .ToListAsync();

        await transaction.CommitAsync();
        return result;
    }

}