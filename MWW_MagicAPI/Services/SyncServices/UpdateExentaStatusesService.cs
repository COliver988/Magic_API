using Hangfire;
using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;
using MWW_Api.Repositories.Magic;
using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services.SyncServices;

public class UpdateExentaStatusesService : IUpdateExentaStatusesService
{
    private readonly IShopfloorDbContextFactory _contextFactory;
    private readonly IMilestoneMapperRepository _milestoneMapperRepository;
    private readonly ISFCTimestampRepository _sfcTimestampRepository;
    private readonly IServiceScopeFactory _scopeFactory;
    private ILogger<UpdateExentaStatusesService> _logger;
    //private List<string> _shopfloors = new List<string>() { "HV" };
    private List<string> _shopfloors = new List<string>() { "HV", "PD", "TJ", "GM" };
    private List<MilestoneMapper> _milestoneMappings;
    private List<SFCTimestamp> _sfcTimestamps;

    public UpdateExentaStatusesService(IShopfloorDbContextFactory contextFactory,
        ILogger<UpdateExentaStatusesService> logger,
        IMilestoneMapperRepository milestoneMapperRepository,
        ISFCTimestampRepository sfcRepo,
        IServiceScopeFactory serviceScopeFactory,
        IEnumerable<ISyncService> workers)
    {
        _contextFactory = contextFactory;
        _milestoneMapperRepository = milestoneMapperRepository;
        _sfcTimestampRepository = sfcRepo;
        _scopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    // TODO: track last timestamp in DBs for update span to only update what is new
    [Queue("datasync")]
    [DisableConcurrentExecution(timeoutInSeconds: 600)]
    public async Task<List<SyncDataResults>> UpdateExentaStatuses(string minutes)
    {
        // load the milestones mappings
        _milestoneMappings = await _milestoneMapperRepository.GetAllMilestoneMappingsAsync();

        // load SFC timestamps
        _sfcTimestamps = await _sfcTimestampRepository.GetAllAsync();

        // get all data to update from shopfloor DBs
        int timeBack = 15;
        if (minutes != null)
        {
           int.TryParse(minutes, out int mins);
           timeBack = mins;
        }
        List<UpdateData> data = await CollectData(timeBack);
        if (data.Count == 0)
        {
            _logger.LogInformation("No Exenta status updates found.");
            return new List<SyncDataResults>();
        }

        using var scope = _scopeFactory.CreateScope();
        IUpdateSyncDataService updateDataService = scope.ServiceProvider.GetRequiredService<IUpdateSyncDataService>();
        // add any other data we need for updates (customer PO, etc)
        data = await updateDataService.UpdateSyncData(data);
        _logger.LogInformation("Resolving scoped sync workers and starting sync");
        var scopedWorkers = scope.ServiceProvider.GetRequiredService<IEnumerable<ISyncService>>().Where(s => s.IsActive);
        var tasks = scopedWorkers.Select(w => w.SyncData(data, _milestoneMappings));
        List<SyncDataResults>[] results = await Task.WhenAll(tasks);
        _logger.LogInformation($"Sync workers completed; total updates: {results.Sum(a => a.Count)}");
        
        // and return a single list of results
        return results.SelectMany(r => r).ToList();
    }

    /// <summary>
    /// get all the data to update for all shopfloor DBs
    /// </summary>
    /// <param name="minutes">number of minutes to go back if no timestamp record</param>
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
            var results = await GetUpdateData(minutes, sf);
            if (results.Count > 0)
                await UpdateSFCTimestamp(sf, results.Max(r => r.Created));
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
    /// <param name="minutes">number of minutes to search back into if no timestamp record</param>
    /// <param name="context">specific shopfloor DB instance</param>
    /// <returns></returns>
    public async Task<List<UpdateData>> GetUpdateData(int minutes, string shopfloorCode)
    {
        ShopfloorDbContext context = _contextFactory.GetContext(shopfloorCode);
        DateTime cutoff = GetLastCheckedUtc(minutes, shopfloorCode);

        var query =
            from u in context.Units.AsNoTracking()
            join t in context.Transactions.AsNoTracking()
                on u.Id equals t.UnitId into ut
            from t in ut.DefaultIfEmpty()              // translates the LEFT JOIN
            join wo in context.WorkOrders.AsNoTracking()
                on t.WorkorderId equals wo.Id
            join po in context.ProductOperations.AsNoTracking()
                on new { OperationId = t.OperationId, wo.ProductId }
                equals new { po.OperationId, po.ProductId }
            join m in context.MileStones.AsNoTracking()
                on po.MileStoneId equals m.Id
            where m.Name.ToLower() != "ready" && t.DateTime > cutoff
            orderby t.Created descending              // optional – matches your original SQL
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
            return results.GroupBy(x => x.AlphaNumId).Select(g => g.First()).ToList(); // ensure distinct by SerialNumber   
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving update data: {ex.Message}");
            return new List<UpdateData>();
        }
    }

    public DateTime GetLastCheckedUtc(int minutes, string shopfloorCode)
    {
        SFCTimestamp? timestamp = _sfcTimestamps.FirstOrDefault(s => s.Location == shopfloorCode);
        return timestamp?.LastChecked ?? DateTime.UtcNow.AddMinutes(-minutes);
    }

    /// <summary>
    /// update the last timestamp for the specified Shopfloor DB
    /// only called if we had any data; no data for this DB should not call this method
    /// </summary>
    /// <param name="shopfloorCode">shoploor code</param>
    /// <param name="lastCheck">last timestamp of the last record</param>
    /// <returns></returns>
    public async Task UpdateSFCTimestamp(string shopfloorCode, DateTime lastCheck)
    {
        SFCTimestamp? timestamp = _sfcTimestamps.FirstOrDefault(s => s.Location == shopfloorCode);
        if (timestamp != null)
        {
            timestamp.LastChecked = lastCheck;
            await _sfcTimestampRepository.UpdateAsync(timestamp);
        }
        else
        {
            SFCTimestamp newTimestamp = new SFCTimestamp
            {
                Location = shopfloorCode,
                LastChecked = lastCheck
            };
            await _sfcTimestampRepository.UpdateAsync(newTimestamp);
        }
    }
}