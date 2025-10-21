using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MWW_Api.Config;
using MWW_Api.Models.Shopfloor;
using MWW_Api.Repositories.Exenta;
using MWW_MagicAPI.Data.Models.DTO;
using System;
using System.Text;

namespace MWW_MagicAPI.Services;

public class FixBatchService : IFixBatchService
{
    private readonly IShopfloorDbContextFactory _contextFactory;
    private IConfiguration _configuration;
    private readonly ExentaDbContext _exentaContext;
    private readonly MagicDbContext _magicDbContext;
    private IGetBatchUnitValues _getBatchUnitValues;
    private ShopfloorDbContext _context;
    private string _timeStamp;

    public FixBatchService(IShopfloorDbContextFactory contextFactory,
        ExentaDbContext exentaContext,
        MagicDbContext magicDbContext,
        IGetBatchUnitValues getBatchUnitValues,
        IConfiguration configuration)
    {
        _contextFactory = contextFactory;
        _exentaContext = exentaContext;
        _magicDbContext = magicDbContext;
        _getBatchUnitValues = getBatchUnitValues;
        _configuration = configuration;
        _timeStamp = DateTime.Now.ToString("MMddyyyyHHmmss");
    }

    public async Task<List<WorkOrderDataDTO>?> GetMissingBatches(string batchId)
    {
        // get the correct Shopfloor context
        _context = _contextFactory.GetContext(batchId);
        List<WorkOrderDataDTO> workorderData = await gatherWorkorderData(batchId);
        write_to_workorder_file(workorderData, batchId);
        await write_to_workorder_units_file(batchId);

        return workorderData;
    }

    private async Task<List<WorkOrderDataDTO>> gatherWorkorderData(string batchId)
    {
        List<MagicUnit> missingUnits = await getMissingBatches(batchId);
        if (!missingUnits.Any()) return null;
        return await getWorkorderData(missingUnits);
    }

    private async Task<List<WorkOrderDataDTO>> getWorkorderData(List<MagicUnit> missingUnits)
    {
        var semaphore = new SemaphoreSlim(10); // Limit to 10 concurrent tasks
        var tasks = new List<Task<WorkOrderDataDTO?>>();

        foreach (var unit in missingUnits)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await _getBatchUnitValues.GetBatchUnitValue(unit.ProdNoCompany, unit.OpenSeq.Value);
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }
        WorkOrderDataDTO?[] results = await Task.WhenAll(tasks);

        return results
            .Where(dto => dto != null)
            .Select(dto => dto!)
            .ToList();
    }

    /// <summary>
    /// write to file; 1st to temp file, then move to final location to prevent hot folder grabbing incomplete file
    /// </summary>
    /// <param name="workorderFileData"></param>
    private void write_to_workorder_file(List<WorkOrderDataDTO> workorderData, string batchId)
    {
        if (workorderData.IsNullOrEmpty()) return;
        string tempFilePath = Path.Combine(Path.GetTempFileName());
        using (StreamWriter writer = new StreamWriter(tempFilePath, false, Encoding.UTF8))
        using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(workorderData);
        }

        // mive to final location and cleanup
        if (File.Exists(tempFilePath))
        {
            string finalFilePath = Path.Combine($"{getPath(batchId)}", $"MWW-{_timeStamp}-Workorder.Exenta");
            Directory.CreateDirectory(Path.GetDirectoryName(finalFilePath)!);
            if (File.Exists(finalFilePath))
                File.Delete(finalFilePath);
            File.Move(tempFilePath, finalFilePath);
            File.Delete(tempFilePath);
        }
    }

    private async Task write_to_workorder_units_file(string batchId)
    {
        string tempFilePath = Path.Combine(Path.GetTempFileName());
        Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath)!);
        List<WorkOrderUnitData> unitData = await GetFormattedPrintDetails(batchId);

        using (StreamWriter writer = new StreamWriter(tempFilePath, false, Encoding.UTF8))
        using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(unitData);
        }
        if (File.Exists(tempFilePath))
        {
            string finalFilePath =  Path.Combine($"{getPath(batchId)}", $"{batchId}-WorkorderUnits.MWW");
            Directory.CreateDirectory(Path.GetDirectoryName(finalFilePath)!);
            if (File.Exists(finalFilePath))
                File.Delete(finalFilePath);
            File.Move(tempFilePath, finalFilePath);
            File.Delete(tempFilePath);
        }
    }

    private async Task<List<WorkOrderUnitData>> GetFormattedPrintDetails(string batchId)
    {
        return await (from dpd in _magicDbContext.DyePrintDetails
                      join ack in _magicDbContext.ExentaPOLinesWithAckNos
                          on new { dpd.PO, LineNo = (int)dpd.Ln_No } equals new { ack.PO, LineNo = ack.LN_NO }
                      join doh in _magicDbContext.DyePrintHeaders
                          on dpd.PO equals doh.PO
                      from dih in _magicDbContext.DyeItemAttributes
                      where dpd.BatchID == batchId
                            && !new[] { "queue", "ready", "pods" }.Contains(dpd.Status)
                            && (doh.ItemCode == dih.MWWItemCode || doh.ItemCode == (dih.MWWItemCode + "-1"))
                      orderby dpd.printedOrder
                      select new WorkOrderUnitData
                      {
                          Workorder = ack.ProdNoCompany + "-" + ack.OpenSeq.ToString(),
                          Batch = dpd.BatchID,
                          Seq = (int)dpd.printedOrder,
                          Unit = $"{dpd.BatchID}_{dpd.printedOrder}",
                          Thumbnail = $"{dpd.PO}_{dpd.printedOrder}.jpg",
                          Quantity = 1,
                          Flag = "I",
                          ProductId = dih.Style,
                          Size = dih.Size,
                      }).ToListAsync();
    }
    /*
    private async Task<List<WorkOrderUnitData>> GetFormattedPrintDetails(string batchId)
    {
        return await (from dpd in _magicDbContext.DyePrintDetails
                      join ack in _magicDbContext.ExentaPOLinesWithAckNos
                          on new { dpd.PO, LineNo = (int)dpd.Ln_No } equals new { ack.PO, LineNo = ack.LN_NO }
                      join doh in _magicDbContext.DyePrintHeaders
                          on dpd.PO equals doh.PO
                      join dih in _magicDbContext.DyeItemAttributes
                          on doh.ItemCode equals dih.MWWItemCode
                      where dpd.BatchID == batchId
                            && !new[] { "queue", "ready", "pods" }.Contains(dpd.Status)
                      orderby dpd.printedOrder
                      select new WorkOrderUnitData
                      {
                          Workorder = ack.ProdNoCompany + "-" + ack.OpenSeq.ToString(),
                          Batch = dpd.BatchID,
                          Seq = (int)dpd.printedOrder,
                          Unit = $"{dpd.BatchID}_{dpd.printedOrder}",
                          Thumbnail = $"{dpd.PO}_{dpd.printedOrder}.jpg",
                          Quantity = 1,
                          Flag = "I",
                      }).ToListAsync();
    }
    */

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

        return filteredMagicUnits;
    }

    private async Task<List<Unit>> getUnitsFromShopfloor(string batchId)
    {
        var units = await _context.Units
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

    private string getPath(string batchID)
    {
        var shopfloor = _configuration.GetSection("Shopfloor");
        string path = shopfloor?[batchID.Substring(0, 2).ToLower()];
        if (path.IsNullOrEmpty())
            path = shopfloor?["mww"];
        return path;
    }
}