using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Repositories.Magic;
using System;
using System.Threading.Tasks;

namespace MWW_MagicAPI.Services;

public class UpdateExentaStatusesService : IUpdateExentaStatusesService
{
    private readonly IShopfloorDbContextFactory _contextFactory;
    private readonly MagicDbContext _magicContext;
    private ILogger<UpdateExentaStatusesService> _logger;
    private List<string> _shopfloors  = new List<string>() { "HV", "PD", "TJ", "GM" };
    private List<string> excludedStatuses = new List<string>() { "shipped", "cancelled", "cancel", "ready", "pods", "toqc", "stship" };
    string[] _progression = new string[]
    {
        "printed", "Printed", "PRINTED", "ToTenter", "AtTenter",
        "InTenter", "waitingLoom", "ToStretch", "stretch", "inLoom",
        "ToCut", "toCut", "ToPack", "ToPB", "ToProc", "ToFinishing", 
        "InPB", "ToSew", "InSew", "ToCircleTack", "ToShip", "cancel"
    };

    public record UpdateData
    {
        public string AlphaNumId { get; set; }
        public string MilestoneName { get; set; }
        public long OperationId { get; set; }
        public long ProductId { get; set; }
        public string SerialNumber { get; set; }
        public DateTime Created { get; set; }
    }

    public record LegacyData
    {
        public string Po { get; set; }
        public string Co { get; set; }
        public string CcApproved { get; set; }
        public string LnNo { get; set; }
        public string Status { get; set; }
        public string BatchSeq { get; set; }
    }

    public UpdateExentaStatusesService(IShopfloorDbContextFactory contextFactory,
        ILogger<UpdateExentaStatusesService> logger,
        MagicDbContext magicContext)
    {
        _contextFactory = contextFactory;
        _magicContext = magicContext;
        _logger = logger;
    }

    public async Task<bool> UpdateExentaStatuses(int minutes)
    {
        // get all data to update from shopfloor DBs
        List<UpdateData> data = CollectData(minutes);
        if (data.Count == 0)
        {
            _logger.LogInformation("No Exenta status updates found.");
            return true;
        }
        // get existing magic data
        List<LegacyData> legacyData = await GetLegacyData();

        return await UpdateMagic(data, legacyData);
    }

    /// <summary>
    /// update the magic DB with the new status
    /// </summary>
    /// <param name="data">data to update</param>
    /// <param name="overlap">pos in magic so only update these</param>
    /// <returns></returns>
    private async Task<bool> UpdateMagic(List<UpdateData> data, List<LegacyData> currentData)
    {
        foreach (LegacyData current in currentData)
        {
            if (!data.Any(d => d.SerialNumber == current.Po))
            {
                _logger.LogInformation($"No matching shopfloor data found for PO: {current.Po}, skipping update.");
                continue;
            }
            UpdateData toUpdate = data.First(d => d.SerialNumber == current.Po);
            int currentIdx = Array.IndexOf(_progression, current.Status);
            int newIdx = Array.IndexOf(_progression, toUpdate.MilestoneName);
            if (newIdx <= currentIdx)
            {
                _logger.LogInformation($"Current status for PO: {current.Po}, CO: {current.Co}, LN: {current.LnNo} is more advanced ({current.Status}) than new status ({toUpdate.MilestoneName}), skipping update.");
                continue;
            }
            
            bool updated = await UpdateMagicStatus(toUpdate);
            if (!updated)
                _logger.LogError($"Failed to update status for PO: {current.Po}, CO: {current.Co}, LN: {current.LnNo} to {toUpdate.MilestoneName}.");
        }
        return true;
    }

    private async Task<bool> UpdateMagicStatus(UpdateData updateData)
    {
        return true;
    }

    /// <summary>
    /// get all the data to update for all DBs
    /// </summary>
    /// <param name="minutes"></param>
    /// <returns>distinct PO / serial number data to update</returns>
    public List<UpdateData> CollectData(int minutes)
    {
        List<UpdateData> data = new();
        foreach (string sf in _shopfloors)
        {
            _logger.LogInformation($"Exenta update status starting for {sf}");
            data.AddRange(GetUpdateData(minutes, _contextFactory.GetContext(sf)));
            _logger.LogInformation($"Exenta update status ending for {sf}");
        }
        return data.DistinctBy(x => x.SerialNumber).ToList();
    }

    /// <summary>
    /// get the update data for a specific Shopfloor DB instance
    /// </summary>
    /// <param name="minutes"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private List<UpdateData> GetUpdateData(int minutes, ShopfloorDbContext context)
    {
        // Calculate cutoff time (equivalent to DATEADD(minute, -args.time, GETUTCDATE()))
        DateTime cutoff = DateTime.UtcNow.AddMinutes(-minutes);

        var query =
            from u in context.Units.AsNoTracking()
            join t in context.Transactions.AsNoTracking()
                on u.Id equals t.UnitId into ut // LEFT JOIN
            from t in ut.DefaultIfEmpty()
            join wo in context.WorkOrders.AsNoTracking()
                on t.WorkorderId equals wo.Id
            join po in context.ProductOperations.AsNoTracking()
                on new { t.OperationId, wo.ProductId }
                equals new { po.OperationId, po.ProductId }
            join m in context.MileStones.AsNoTracking()
                on po.MileStoneId equals m.Id
            where m.Name != "READY"
                  && t.DateTime >= cutoff
            orderby t.Created descending
            select new UpdateData
            {
                AlphaNumId = u.AlphaNumId,
                MilestoneName = m.Name,
                OperationId = po.OperationId,
                ProductId = po.ProductId,
                SerialNumber = wo.Serialnumber,
                Created = t.Created
            };

        List<UpdateData> results = query.Distinct().ToList();
        return results;
    }

    private async Task<List<LegacyData>> GetLegacyData()
    {
        // Entity Framework LINQ query
        var query = from dpd in _magicContext.DyePrintDetails.AsNoTracking()
                    join dp in _magicContext.DapPartners.AsNoTracking()
                        on dpd.PO equals dp.PO
                    where !excludedStatuses.Contains(dpd.Status)
                    select new LegacyData
                    {
                        Po = dpd.PO,
                        Co = dpd.CO_Number,
                        CcApproved = dp.CC_APPROVED,
                        LnNo = dpd.Ln_No.ToString(),
                        Status = dpd.Status,
                        BatchSeq = dpd.BatchID + "_" + dpd.PrintOrder
                    };

        // Execute query (e.g., ToList)
        return await query.ToListAsync();
    }
}