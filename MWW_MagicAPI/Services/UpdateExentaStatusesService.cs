using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
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
        List<UpdateData> data = await CollectData(minutes);
        if (data.Count == 0)
        {
            _logger.LogInformation("No Exenta status updates found.");
            return true;
        }
        // get existing magic data
        //List<LegacyData> legacyData = await GetLegacyData();

        return await UpdateMagic(data);
    }

    /// <summary>
    /// update the magic DB with the new status
    /// </summary>
    /// <param name="data">data to update</param>
    /// <param name="overlap">pos in magic so only update these</param>
    /// <returns></returns>
    private async Task<bool> UpdateMagic(List<UpdateData> data)
    {
        Dictionary<string, List<string>> updateOrders = new(); // status and list of POs to be updated to the new status
        foreach (UpdateData toUpdate in data)
        {
            LegacyData? current = await _magicContext.DyePrintDetails.AsNoTracking()
                .Where(dpd => dpd.PO == toUpdate.SerialNumber && !excludedStatuses.Contains(dpd.Status))
                .Join(_magicContext.DapPartners.AsNoTracking(),
                    dpd => dpd.PO,
                    dp => dp.PO,
                    (dpd, dp) => new LegacyData
                    {
                        Po = dpd.PO,
                        Co = dpd.CO_Number,
                        CcApproved = dp.CC_APPROVED,
                        LnNo = dpd.Ln_No.ToString(),
                        Status = dpd.Status,
                        BatchSeq = dpd.BatchID + "_" + dpd.PrintOrder
                    })
                .FirstOrDefaultAsync();
            if (current != null)
            {
                int currentIdx = Array.FindIndex(_progression, s => string.Equals(s, current.Status, StringComparison.OrdinalIgnoreCase));
                int newIdx = Array.FindIndex(_progression, s => string.Equals(s, toUpdate.MilestoneName, StringComparison.OrdinalIgnoreCase));
                if (newIdx > currentIdx)
                    AddToUpdatedOrders(toUpdate.MilestoneName, toUpdate.SerialNumber, updateOrders);
            }
        }
        /*
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
                _logger.LogInformation($"Current idx: {currentIdx}, New idx: {newIdx}");
                continue;
            }
            
            AddToUpdatedOrders(toUpdate.MilestoneName, current.Po, updateOrders);
        }
        */

        // now update the magic DB
        return await UpdateMagicStatus(updateOrders);
    }

    /// <summary>
    /// add PO to the list of orders to be updated for a specific milestone
    /// </summary>
    /// <param name="milestoneName"></param>
    /// <param name="po"></param>
    /// <param name="updateOrders"></param>
    private void AddToUpdatedOrders(string milestoneName, string po, Dictionary<string, List<string>> updateOrders)
    {
        if (updateOrders.ContainsKey(milestoneName))
        {
            updateOrders[milestoneName].Add(po);
        }
        else
        {
            updateOrders[milestoneName] = new List<string> { po };
        }
    }

    private async Task<bool> UpdateMagicStatus(Dictionary<string, List<string>> updateData)
    {
        return true;
    }

    /// <summary>
    /// get all the data to update for all DBs
    /// </summary>
    /// <param name="minutes"></param>
    /// <returns>distinct PO / serial number data to update</returns>
    public async Task<List<UpdateData>> CollectData(int minutes)
    {
        List<UpdateData> data = new();
        foreach (string sf in _shopfloors)
        {
            _logger.LogInformation($"Exenta update status starting for {sf}");
            data.AddRange(await GetUpdateData(minutes, _contextFactory.GetContext(sf)));
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
    private async Task<List<UpdateData>> GetUpdateData(int minutes, ShopfloorDbContext context)
    {
        // Calculate cutoff time (equivalent to DATEADD(minute, -args.time, GETUTCDATE()))
        DateTime cutoff = DateTime.UtcNow.AddMinutes(-minutes);

        var query =
            from m in context.MileStones.AsNoTracking()
            where m.Name != "READY"
            join po in context.ProductOperations.AsNoTracking()
                on m.Id equals po.MileStoneId
            join wo in context.WorkOrders.AsNoTracking()
                on po.ProductId equals wo.ProductId
            join t in context.Transactions.AsNoTracking()
                on new { wo.Id, po.OperationId } equals new { Id = t.WorkorderId, t.OperationId }
            where t.DateTime >= cutoff
            join u in context.Units.AsNoTracking()
                on t.UnitId equals u.Id
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

        try
        {
            List<UpdateData> results = await query.Distinct().ToListAsync();
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving update data: {ex.Message}");
            return new List<UpdateData>();
        }   
    }

    /// <summary>
    /// get existing magic data to compare against; only gets "active" orders
    /// </summary>
    /// <returns>list of orders that may need updating</returns>
    private async Task<List<LegacyData>> GetLegacyData()
    {
        var query = from dpd in _magicContext.DyePrintDetails.AsNoTracking()
                    join dp in _magicContext.DapPartners.AsNoTracking()
                        on dpd.PO equals dp.PO
                    where !excludedStatuses.Contains(dpd.Status)
                    where dp.DATE_PLACED >= DateTime.UtcNow.AddMonths(-3) // only get recent orders
                    select new LegacyData
                    {
                        Po = dpd.PO,
                        Co = dpd.CO_Number,
                        CcApproved = dp.CC_APPROVED,
                        LnNo = dpd.Ln_No.ToString(),
                        Status = dpd.Status,
                        BatchSeq = dpd.BatchID + "_" + dpd.PrintOrder
                    };

        return await query.ToListAsync();
    }
}