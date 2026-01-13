using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;
using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services.SyncServices;

public class MagicSyncService : ISyncService
{
    private readonly ILogger<MagicSyncService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private List<MilestoneMapper> _mappings;
    private List<string> excludedStatuses = new List<string>() { "shipped", "cancelled", "cancel", "ready", "pods", "toqc", "stship" };
    string[] _progression = new string[]
    {
        "printed", "Printed", "PRINTED", "ToTenter", "AtTenter",
        "InTenter", "waitingLoom", "ToStretch", "stretch", "inLoom",
        "ToCut", "toCut", "ToPack", "ToPB", "ToProc", "ToFinishing", 
        "InPB", "ToSew", "InSew", "ToCircleTack", "ToShip", "cancel"
    };

    public MagicSyncService(
        ILogger<MagicSyncService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<int> SyncData(List<UpdateData> data, 
        List<MilestoneMapper> mappings)
    {
        _mappings = mappings;
        return await UpdateMagicStatuses(data);
    }

    public async Task<int> UpdateMagicStatuses(List<UpdateData> data)
    { 
        List<LegacyData> updateOrders = new(); // will contain legacy rows with Status set to target status
        var semaphore = new SemaphoreSlim(5); // max concurrency 5
        var addLock = new object();

        var tasks = data.Select(async toUpdate =>
        {
            await semaphore.WaitAsync();
            try
            {
                LegacyData? current = await LoadLegacyData(toUpdate.SerialNumber);
                if (current != null)
                {
                    int currentIdx = Array.FindIndex(_progression, s => string.Equals(s, current.Status, StringComparison.OrdinalIgnoreCase));

                    string newStatus = _mappings
                        .Where(m => string.Equals(m.Milestone, toUpdate.MilestoneName, StringComparison.OrdinalIgnoreCase))
                        .Select(m => m.NewStatus)
                        .FirstOrDefault() ?? current.Status;

                    int newIdx = Array.FindIndex(_progression, s => string.Equals(s, newStatus, StringComparison.OrdinalIgnoreCase));
                    if (newIdx > currentIdx)
                    {
                        // set the target status on the legacy record before adding
                        current.Status = newStatus;
                        lock (addLock)
                        {
                            updateOrders.Add(current);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing update for PO {Po}", toUpdate.SerialNumber);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks);

        return await UpdateMagicDB(updateOrders);
    }

    /// <summary>
    /// load up the existing data from Magic DB
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public async Task<LegacyData?> LoadLegacyData(string po)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        MagicDbContext magicContext = scope.ServiceProvider.GetRequiredService<MagicDbContext>();
        LegacyData? current = await magicContext.DyePrintDetails.AsNoTracking()
            .Where(dpd => dpd.PO == po && !excludedStatuses.Contains(dpd.Status.ToLower()))
            .Join(magicContext.DapPartners.AsNoTracking(),
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

        using var scope = _serviceScopeFactory.CreateScope();
        MagicDbContext magicContext = scope.ServiceProvider.GetRequiredService<MagicDbContext>();

        // Start the transaction
        using var transaction = await magicContext.Database.BeginTransactionAsync();

        try
        {
            // 1. Add records to the Change Tracker for UPCLogs
            int logsAdded = await AddUPCLogs(updateOrders, magicContext);
            if (logsAdded < 0) throw new Exception("Failed to prepare UPC logs.");

            // 2. Add records/updates to the Change Tracker for DyePrintDetails
            int detailsUpdated = await UpdateDyePrintDetails(updateOrders, magicContext);
            if (detailsUpdated < 0) throw new Exception("Failed to prepare DyePrintDetails updates.");

            // 3. Persist all changes to the database at once
            // EF Core wraps SaveChangesAsync in its own internal transaction, 
            // but BeginTransactionAsync ensures nothing is committed until we say so.
            await magicContext.SaveChangesAsync();

            // 4. Commit the transaction to the database
            //await transaction.CommitAsync();

            _logger.LogInformation("Successfully updated {Count} records in Magic DB.", detailsUpdated);
            return detailsUpdated;
        }
        catch (Exception ex)
        {
            // Roll back the transaction if anything fails
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Magic DB transaction failed. All changes rolled back.");
            return -1;
        }
    }

    private async Task<int> UpdateDyePrintDetails(List<LegacyData> updateOrders, MagicDbContext magicContext)
    {
        int result = 0;

        // Build a PO -> target status map for fast lookup
        var poToTarget = updateOrders
            .GroupBy(u => u.Po)
            .ToDictionary(g => g.Key, g => g.First().Status, StringComparer.OrdinalIgnoreCase);

        try
        {
            // Fetch all matching DyePrintDetails in a single query
            var pos = poToTarget.Keys.ToList();
            var details = await magicContext.DyePrintDetails
                .Where(d => pos.Contains(d.PO))
                .ToListAsync();

            if (details.Count == 0)
                return result;

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

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating DyePrintDetails: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// add the UPC_LOG_IN records in a single batch
    /// </summary>
    /// <param name="updateData"></param>
    /// <returns></returns>
    public async Task<int> AddUPCLogs(List<LegacyData> updateData, MagicDbContext magicContext)
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

            magicContext.UPCLogIns.AddRange(batchRecords);

            return updateData.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding UPC logs: {ex.Message}");
            return -1;
        }
    }

}
