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

    public async Task<int> UpdateExentaStatuses(int minutes)
    {
        // get all data to update from shopfloor DBs
        List<UpdateData> data = await CollectData(minutes);
        if (data.Count == 0)
        {
            _logger.LogInformation("No Exenta status updates found.");
            return 0;
        }
        return await UpdateMagic(data);
    }

    /// <summary>
    /// update the magic DB with the new status
    /// </summary>
    /// <param name="data">data to update</param>
    /// <param name="overlap">pos in magic so only update these</param>
    /// <returns></returns>
    public async Task<int> UpdateMagic(List<UpdateData> data)
    {
        List<LegacyData> updateOrders = new(); // status and list of POs to be updated to the new status
        foreach (UpdateData toUpdate in data)
        {
            LegacyData? current = await LoadLegacyData(toUpdate.SerialNumber);
            if (current != null)
            {
                int currentIdx = Array.FindIndex(_progression, s => string.Equals(s, current.Status, StringComparison.OrdinalIgnoreCase));
                int newIdx = Array.FindIndex(_progression, s => string.Equals(s, toUpdate.MilestoneName, StringComparison.OrdinalIgnoreCase));
                if (newIdx > currentIdx)
                    updateOrders.Add(current); 
            }
        }
        return await UpdateMagicDB(updateOrders);
    }

    public async Task<LegacyData?> LoadLegacyData(string po)
    {
        LegacyData? current = await _magicContext.DyePrintDetails.AsNoTracking()
            .Where(dpd => dpd.PO == po && !excludedStatuses.Contains(dpd.Status.ToLower()))
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
        return current;
    }

    public async Task<int> UpdateMagicDB(List<LegacyData> updateOrders)
    {
        if (updateOrders.Count == 0)
        {
            _logger.LogInformation("No Magic DB updates required.");
            return 0;
        }
        int result = await AddUPCLogs(updateOrders);
        if (result > 0)
            result = await UpdateDyePrintDetails(updateOrders);
        return result;
    }

    private async Task<int> UpdateDyePrintDetails(List<LegacyData> updateOrders)
    {
        int result = 0;

        // Build a PO -> target status map for fast lookup
        var poToTarget = updateOrders
            .GroupBy(u => u.Po)
            .ToDictionary(g => g.Key, g => g.First().Status, StringComparer.OrdinalIgnoreCase);

        // Use transaction to keep updates atomic
        using var tx = await _magicContext.Database.BeginTransactionAsync();
        try
        {
            // Fetch all matching DyePrintDetails in a single query
            var pos = poToTarget.Keys.ToList();
            var details = await _magicContext.DyePrintDetails
                .Where(d => pos.Contains(d.PO))
                .ToListAsync();

            if (details.Count == 0)
            {
                await tx.CommitAsync();
                return result;
            }

            // Update each entity's status (and any audit columns if present)
            var utcNow = DateTime.UtcNow;
            foreach (var detail in details)
            {
                if (!poToTarget.TryGetValue(detail.PO, out var targetStatus))
                    continue;

                // Only set when different — avoids unnecessary writes
                if (!string.Equals(detail.Status, targetStatus, StringComparison.Ordinal))
                {
                    detail.Status = targetStatus;
                    result++;
                }
            }

            await _magicContext.SaveChangesAsync();
            await tx.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError($"Error updating DyePrintDetails: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// add the UPC_LOG_IN records in a single batch
    /// </summary>
    /// <param name="updateData"></param>
    /// <returns></returns>
    public async Task<int> AddUPCLogs(List<LegacyData> updateData)
    {
        try
        {
            var batchRecords = updateData.Select(entry => new UPCLogIn
            {
                CUST_PO_NO = entry.Po,
                CO_NUMBER = entry.Co,
                CUST_ID = entry.UserId,
                USERID = entry.Status,
                SHIP_VIA = entry.LineNumber,
                CreateDate = DateTime.UtcNow,
                LOG_DATE = DateTime.UtcNow.ToString("yyyyMMdd"),
                SYSTEM_NAME = "MWWMagicAPI",
            }).ToList();

            _magicContext.UPCLogIns.AddRange(batchRecords);

            //await _magicContext.SaveChangesAsync();
            return updateData.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding UPC logs: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// get all the data to update for all shopfloor DBs
    /// </summary>
    /// <param name="minutes">number of minutes to go back</param>
    /// <returns>distinct UpdatData records for possible updates</returns>
    /// <note>duplicates across shopfloor DBs are removed</note>
    public async Task<List<UpdateData>> CollectData(int minutes)
    {
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5 };
        var lockObject = new object();
        List<UpdateData> data = new();

        await Parallel.ForEachAsync(_shopfloors, parallelOptions, async (sf, ct) =>
        {
            _logger.LogInformation($"Exenta update status starting for {sf}");
            var results = await GetUpdateData(minutes, _contextFactory.GetContext(sf));
            lock (lockObject)
            {
                data.AddRange(results);
            }
            _logger.LogInformation($"Exenta update status ending for {sf}");
        });

        return data.DistinctBy(x => x.SerialNumber).ToList();
    }

    /// <summary>
    /// get the update data for a specific Shopfloor DB instance
    /// </summary>
    /// <param name="minutes">number of minutes to search back into</param>
    /// <param name="context">specific shopfloor DB instance</param>
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