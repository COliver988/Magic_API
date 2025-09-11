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
        StringBuilder workorderFileData = new StringBuilder();
        foreach (MagicUnit unit in missingUnits)
            workorderFileData.AppendLine(await batchUnitValues(unit.ProdNoCompany, unit.OpenSeq) + ",");

        return null;
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

    private async Task<string> batchUnitValues(int prodNoCompany, int openSeq)
    {
        List<Dictionary<string, string>> exentaData = await getExentaOrderDataAsync(prodNoCompany);
        string consolidate = goesToConsolidation(exentaData);
        Dictionary<string, string> unitData =  await getExentaUnitDataAsync(prodNoCompany, openSeq, consolidate);
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

    private async Task<Dictionary<string, string>> getExentaUnitDataAsync(int prodNoCompany, int sequence, string consolidate)
    {
        using var transaction = await _exentaContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var query = from pod in _exentaContext.ProdOrderDetails
                    join poh in _exentaContext.ProdOrderHeaders on pod.PRODNO equals poh.PRODNO
                    join pioh in _exentaContext.PickOrderHeaders on poh.ORDERNO equals pioh.ORDERNO
                    join piod in _exentaContext.PickOrderDetails on new { pioh.PICKNO, pod.ORDERSEQ } equals new { piod.PICKNO, ORDERSEQ = piod.SEQUENCE }
                    join si in _exentaContext.StyleItems on pod.ITEMNO equals si.ITEMNO
                    join st in _exentaContext.Styles on si.STYLE equals st.STYLE
                    join s in _exentaContext.Sizes on si.SIZE equals s.SIZE into sizeJoin
                    from s in sizeJoin.DefaultIfEmpty()
                    join d in _exentaContext.Dimensions on si.DIMENSION equals d.DIMENSION into dimJoin
                    from d in dimJoin.DefaultIfEmpty()
                    join c in _exentaContext.Colors on si.COLOR equals c.COLOR into colorJoin
                    from c in colorJoin.DefaultIfEmpty()
                    where poh.PRODNOCOMPANY == prodNoCompany && pod.OPENSEQ == sequence && pod.PRODSTAGENO == 1
                    select new
                    {
                        pod.PRODSTAGENO,
                        poh.PRODNOCOMPANY,
                        pod.OPENSEQ,
                        pod.ITEMNO,
                        si.STYLE,
                        st.STYLENAME,
                        si.LABEL,
                        si.COLOR,
                        c.COLORDESC,
                        si.DIMENSION,
                        d.DIMENSIONDESC,
                        si.SIZE,
                        s.SIZEDESC,
                        PRODLINEQTY = (int?)pod.PRODLINEQTY,
                        pod.UOM,
                        DETAILSHIPDATE = pod.SHIPDATE.HasValue ? pod.SHIPDATE.Value.ToString("MM/dd/yyyy") : "",
                        DTLDUEDATE = pod.DUEDATE.HasValue ? pod.DUEDATE.Value.ToString("MM/dd/yyyy") : "",
                        poh.ORDERNOCOMPANY,
                        piod.WEBUDF03,
                        poh.WAREHOUSE,
                        Consolidate = consolidate,
                        Message = ""
                    };

        var result = await query.FirstOrDefaultAsync();

        await transaction.CommitAsync();

        return result?.GetType()
            .GetProperties()
            .ToDictionary(
                prop => prop.Name,
                prop => (prop.GetValue(result)?.ToString() ?? "").Trim()
            ) ?? new Dictionary<string, string>();
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