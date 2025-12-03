using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;

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
        public string LnNo { get; set; }
        public string Status { get; set; }
        public string BatchSeq { get; set; }
        public string UserId { get; set; }
        public string LineNumber { get; set; }
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
        List<LegacyData> updateOrders = new(); // status and list of POs to be updated to the new status
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
                        LnNo = dpd.Ln_No.ToString(),
                        Status = dpd.Status,
                        UserId = dp.CC_APPROVED, // legacy vendor ID aka CUID
                        LineNumber = dpd.Ln_No.ToString(),
                        BatchSeq = dpd.BatchID + "_" + dpd.PrintOrder
                    })
                .FirstOrDefaultAsync();
            if (current != null)
            {
                int currentIdx = Array.FindIndex(_progression, s => string.Equals(s, current.Status, StringComparison.OrdinalIgnoreCase));
                int newIdx = Array.FindIndex(_progression, s => string.Equals(s, toUpdate.MilestoneName, StringComparison.OrdinalIgnoreCase));
                if (toUpdate.MilestoneName != "COMPLETE" && currentIdx != newIdx)
                {
                    string hi = "hi";
                }
                if (newIdx > currentIdx)
                    updateOrders.Add(current); 
            }
        }
        // now update the magic DB
        return await UpdateMagicDB(updateOrders);
    }

    private async Task<bool> UpdateMagicDB(List<LegacyData> updateOrders)
    {
        bool result = await AddUPCLogs(updateOrders);
        //if (result)
        //    result = await UpdateDyePrintDetails(updateData);
        return result;
    }

    /// <summary>
    /// add the UPC_LOG_IN records
    /// </summary>
    /// <param name="updateData"></param>
    /// <returns></returns>
    public async Task<bool> AddUPCLogs(List<LegacyData> updateData)
    {
        try
        {
            foreach (LegacyData entry in updateData)
            {
                _magicContext.UPCLogIns.Add(new UPCLogIn
                {
                    CUST_PO_NO = entry.Po,
                    CO_NUMBER = entry.Po,
                    CUST_ID = entry.UserId,
                    USERID = entry.Status,
                    SHIP_VIA = entry.LineNumber,
                    CreateDate = DateTime.UtcNow,
                    LOG_DATE = DateTime.UtcNow.ToString("yyyyMMdd"),
                    SYSTEM_NAME = "MWWMagicAPI",
                });
            }
            await _magicContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding UPC logs: {ex.Message}");
            return false;
        }
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
}