using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_Api.Repositories.Exenta;

public class GetBatchUnitValues : IGetBatchUnitValues
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private ExentaDbContext _exentaContext;

    public GetBatchUnitValues(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public GetBatchUnitValues(int prodNoCompany, int sequence)
    {
        var scope = _serviceScopeFactory.CreateScope();
    }

    public async Task<WorkOrderDataDTO?> GetBatchUnitValue(int prodNoCompany, int sequence)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        _exentaContext = scope.ServiceProvider.GetRequiredService<ExentaDbContext>();
        List<Dictionary<string, string>> exentaData = await getExentaOrderDataAsync(prodNoCompany);
        string consolidate = goesToConsolidation(exentaData);
        return await getExentaUnitDataAsync(prodNoCompany, sequence, consolidate);
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

    private async Task<WorkOrderDataDTO> getExentaUnitDataAsync(int prodNoCompany, int sequence, string consolidate)
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
        return new WorkOrderDataDTO
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
}
