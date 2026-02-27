using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;
using MWW_MagicAPI.Data.Models.DTO;
using Newtonsoft.Json;
using Prometheus;

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

    public bool IsActive => true;

    public MagicSyncService(
        ILogger<MagicSyncService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<List<SyncDataResults>> SyncData(List<UpdateData> data,
        List<MilestoneMapper> mappings)
    {
        _mappings = mappings;
        return await UpdateMagicStatuses(data);
    }

    public async Task<List<SyncDataResults>> UpdateMagicStatuses(List<UpdateData> data)
    {
        List<LegacyData> updateOrders = new(); // will contain legacy rows with Status set to target status
        var semaphore = new SemaphoreSlim(5); // max concurrency 5
        var addLock = new object();

        var tasks = data.Select(async toUpdate =>
        {
            await semaphore.WaitAsync();
            try
            {
                LegacyData? current = await LoadLegacyData(toUpdate.SerialNumber, toUpdate.AlphaNumId);
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
    public async Task<LegacyData?> LoadLegacyData(string po, string alphanumId)
    {
        using var timer = HistogramMetrics.LegacyLoadDuration
            .WithLabels(nameof(LoadLegacyData))
            .NewTimer();

        using var scope = _serviceScopeFactory.CreateScope();
        var magicContext = scope.ServiceProvider.GetRequiredService<MagicDbContext>();

        var loweredStatuses = excludedStatuses.Select(s => s.ToLower()).ToList();

        var query =
            from dpd in magicContext.DyePrintDetails.AsNoTracking()
            where dpd.PO == po && !loweredStatuses.Contains(dpd.Status.ToLower())
            join dp in magicContext.DapPartners.AsNoTracking().Where(x => x.PO == po)
                on dpd.PO equals dp.PO
            select new LegacyData
            {
                Po = dpd.PO,
                Co = dpd.CO_Number,
                LnNo = dpd.Ln_No,
                Status = dpd.Status,
                UserId = dp.CC_APPROVED,
                LineNumber = dpd.Ln_No.ToString(),
                BatchSeq = alphanumId
            };

        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<SyncDataResults>> UpdateMagicDB(List<LegacyData> updateOrders)
    {
        List<SyncDataResults> results = new();
        if (updateOrders.Count == 0)
        {
            _logger.LogInformation("No Magic DB updates required.");
            return results;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        MagicDbContext magicContext = scope.ServiceProvider.GetRequiredService<MagicDbContext>();

        // Start the transaction
        using var transaction = await magicContext.Database.BeginTransactionAsync();

        try
        {
            // 1. Add records/updates to the Change Tracker for DyePrintDetails
            List<LegacyData> detailsUpdated = await UpdateDyePrintDetails(updateOrders, magicContext);
            if (detailsUpdated.Count == 0) throw new Exception("Failed to prepare DyePrintDetails updates.");

            // 2. Add records to the Change Tracker for UPCLogs
            results = await AddUPCLogs(detailsUpdated, magicContext);
            if (results.Count != detailsUpdated.Count)
            {
                await transaction.RollbackAsync();
                _logger.LogError("Failed to prepare all UPC log records for Printify updates.");
                return results;
            }

            // 3. Persist all changes to the database at once
            // EF Core wraps SaveChangesAsync in its own internal transaction, 
            // but BeginTransactionAsync ensures nothing is committed until we say so.
            await magicContext.SaveChangesAsync();

            // 4. Commit the transaction to the database
            await transaction.CommitAsync();

            var json = JsonConvert.SerializeObject(detailsUpdated, Formatting.Indented);
            _logger.LogInformation("Successfully updated {Count} records in Magic DB.", json);
            return results;
        }
        catch (Exception ex)
        {
            // Roll back the transaction if anything fails
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Magic DB transaction failed. All changes rolled back.");
            return results;
        }
    }

    private async Task<List<LegacyData>> UpdateDyePrintDetails(List<LegacyData> updateOrders, MagicDbContext magicContext)
    {
        List<LegacyData> updatedDetails = new();

        // 1. Create a composite lookup key: "PO-LineNumber"
        // This prevents updating Line 2 when only Line 1 was intended.
        var updateLookup = updateOrders
            .ToDictionary(
                u => $"{u.Po}_{u.LnNo}",
                u => u,
                StringComparer.OrdinalIgnoreCase
            );

        try
        {
            // 2. Extract POs to minimize the initial fetch
            var pos = updateOrders.Select(u => u.Po).Distinct().ToList();

            // 3. Fetch records from DB
            var details = await magicContext.DyePrintDetails
                .Where(d => pos.Contains(d.PO))
                .ToListAsync();

            if (details.Count == 0) return updatedDetails;

            foreach (var detail in details)
            {
                // 4. Match using the composite key (PO + Line Number)
                string key = $"{detail.PO}_{detail.Ln_No}";

                if (updateLookup.TryGetValue(key, out var targetUpdate))
                {
                    // 5. Only update if the status is actually changing
                    if (!string.Equals(detail.Status, targetUpdate.Status, StringComparison.OrdinalIgnoreCase))
                    {
                        detail.Status = targetUpdate.Status;
                        updatedDetails.Add(targetUpdate);
                    }
                }
            }

            return updatedDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating DyePrintDetails specifically by Line Number.");
            return new List<LegacyData>();
        }
    }

    /// <summary>
    /// add the UPC_LOG_IN records in a single batch
    /// </summary>
    /// <param name="updateData"></param>
    /// <returns></returns>
    public async Task<List<SyncDataResults>> AddUPCLogs(List<LegacyData> updateData, MagicDbContext magicContext)
    {
        try
        {
            var batchRecords = updateData.Select(entry => new UPCLogIn
            {
                CUST_PO_NO = entry.Po,
                CO_NUMBER = entry.Co,
                CUST_ID = entry.UserId,
                USERID = entry.Status.ToUpper(),
                SHIP_VIA = entry.LineNumber,
                CreateDate = DateTime.Now,
                LOG_DATE = DateTime.Now.ToString("MMM dd yyyy h:mmtt"),
                SYSTEM_NAME = "MWWMagicAPI",
                TrackNotes = $"{entry.BatchSeq} => {entry.Status}"
            }).ToList();

            batchRecords = await DedupLegacy(batchRecords, magicContext);

            magicContext.UPCLogIns.AddRange(batchRecords);
            var json = JsonConvert.SerializeObject(batchRecords, Formatting.Indented);
            _logger.LogInformation("UPC log records prepared:\n{Json}", json);


            return updateData.Select(d => new SyncDataResults
            {
                PO = d.Po,
                VendorPO = d.Po,
                LnNo = d.LnNo,
                RecordType = "UPCLogIn",
                OldStatus = "",
                NewStatus = d.Status
            })
            .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding UPC logs: {ex.Message}");
            return new List<SyncDataResults>();
        }
    }

    private async Task<List<UPCLogIn>> DedupLegacy(List<UPCLogIn> newRecords, MagicDbContext magicContext)
    {
        var poNos = newRecords.Select(r => r.CUST_PO_NO).Distinct().ToList();
        var existingLogs = await magicContext.UPCLogIns
            .Where(log => poNos.Contains(log.CUST_PO_NO))
            .AsNoTracking()
            .ToListAsync();
        var dedupedRecords = newRecords
            .Where(newLog => !existingLogs.Any(existingLog =>
                existingLog.CUST_PO_NO == newLog.CUST_PO_NO &&
                existingLog.CO_NUMBER == newLog.CO_NUMBER &&
                existingLog.USERID == newLog.USERID &&
                existingLog.TrackNotes == newLog.TrackNotes &&
                existingLog.SHIP_VIA == newLog.SHIP_VIA))
            .ToList();
        return dedupedRecords;
    }
}
