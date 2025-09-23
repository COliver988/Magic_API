using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_MagicAPI.Data.Models.DTO;
using Prometheus;

namespace MWW_Api.Repositories.Exenta;

public class GetBatchUnitValues : IGetBatchUnitValues
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private static readonly Histogram MethodDuration = Metrics
       .CreateHistogram("getExentaOrderDataAsync",
                        "Tracks the duration of getExentaOrderDataAsync in seconds.");

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
        var exentaContext = scope.ServiceProvider.GetRequiredService<ExentaDbContext>();

        var exentaData = await getExentaOrderDataAsync(exentaContext, prodNoCompany);
        var consolidate = goesToConsolidation(exentaData);
        using (MethodDuration.NewTimer())
            return await getExentaUnitDataAsync(exentaContext, prodNoCompany, sequence, consolidate);
    }

    private async Task<List<Dictionary<string, string>>> getExentaOrderDataAsync(ExentaDbContext exentaContext, int prodNoCompany)
    {
        // Optional: Begin a transaction with ReadUncommitted isolation
        using var transaction = await exentaContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var query = from poh in exentaContext.ProdOrderHeaders
                    join pod in exentaContext.ProdOrderDetails
                        on poh.PKEY equals pod.FKEY
                    join oh in exentaContext.OrderHeaders
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

    private async Task<WorkOrderDataDTO> getExentaUnitDataAsync(ExentaDbContext exentaContext, int prodNoCompany, int sequence, string consolidate)
    {
        // Step 1: Get ProdOrderHeader
        var poh = await exentaContext.ProdOrderHeaders
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.PRODNOCOMPANY == prodNoCompany);

        if (poh == null) return null;

        // Step 2: Get ProdOrderDetail
        var pod = await exentaContext.ProdOrderDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PRODSTAGENO == 1 && p.OPENSEQ == sequence && p.PRODNO == poh.PRODNO);

        if (pod == null) return null;


        // Step 3: Get PickOrderHeader
        var pioh = await exentaContext.PickOrderHeaders
            .AsNoTracking()
            .FirstOrDefaultAsync(ph => ph.ORDERNO == poh.ORDERNO);

        // Step 4: Get PickOrderDetail
        var piod = pioh != null
            ? await exentaContext.PickOrderDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(pd => pd.PICKNO == pioh.PICKNO && pd.SEQUENCE == pod.ORDERSEQ)
            : null;

        // Step 5: Get StyleItem
        var si = await exentaContext.StyleItems
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ITEMNO == pod.ITEMNO);

        if (si == null) return null;

        // Step 6: Get Style
        var st = await exentaContext.Styles
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.STYLE == si.STYLE);

        // Step 7: Get Size (optional)
        var s = !string.IsNullOrEmpty(si.SIZE)
            ? await exentaContext.Sizes.AsNoTracking().FirstOrDefaultAsync(sz => sz.SIZE == si.SIZE)
            : null;

        // Step 8: Get Dimension (optional)
        var d = !string.IsNullOrEmpty(si.DIMENSION)
            ? await exentaContext.Dimensions.AsNoTracking().FirstOrDefaultAsync(dim => dim.DIMENSION == si.DIMENSION)
            : null;

        // Step 9: Get Color (optional)
        var c = !string.IsNullOrEmpty(si.COLOR)
            ? await exentaContext.Colors.AsNoTracking().FirstOrDefaultAsync(col => col.COLOR == si.COLOR)
            : null;

        // Step 10: Project the result
        return new WorkOrderDataDTO
        {
            ProdStageNo = pod.PRODSTAGENO,
            ProdNoCompany = poh.PRODNOCOMPANY,
            OpenSeq = pod.OPENSEQ,
            ItemNo = pod.ITEMNO,
            Style = si.STYLE.Trim(),
            StyleName = st?.STYLENAME.Trim(),
            Label = si.LABEL.Trim(),
            Color = si.COLOR.Trim(),
            ColorDesc = c?.COLORDESC.Trim(),
            Dimension = si.DIMENSION.Trim(),
            DimensionDesc = d?.DIMENSIONDESC.Trim(),
            Size = si.SIZE.Trim(),
            SizeDesc = s?.SIZEDESC.Trim(),
            ProdLineQty = (int?)pod.PRODLINEQTY,
            UOM = pod.UOM.Trim(),
            DetailShipDate = pod.SHIPDATE.ToString("MM/dd/yyyy"),
            DtlDueDate = pod.DUEDATE.ToString("MM/dd/yyyy"),
            OrderNoCompany = poh.ORDERNOCOMPANY,
            PONumber = piod?.WEBUDF03.Trim() ?? "",
            Warehouse = poh.WAREHOUSE.Trim(),
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
            return "Y";
        else
            return "N";
    }
}
