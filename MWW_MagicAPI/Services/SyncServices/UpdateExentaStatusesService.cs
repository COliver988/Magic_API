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
    private readonly IServiceScopeFactory _scopeFactory;
    private ILogger<UpdateExentaStatusesService> _logger;
    private List<string> _shopfloors  = new List<string>() { "HV", "PD", "TJ", "GM" };
    private List<MilestoneMapper> _milestoneMappings;
    private readonly IEnumerable<ISyncService> _workers;

    public UpdateExentaStatusesService(IShopfloorDbContextFactory contextFactory,
        ILogger<UpdateExentaStatusesService> logger,
        IMilestoneMapperRepository milestoneMapperRepository,
        IServiceScopeFactory serviceScopeFactory,
        IEnumerable<ISyncService> workers)
    {
        _contextFactory = contextFactory;
        _milestoneMapperRepository = milestoneMapperRepository;
        _scopeFactory = serviceScopeFactory;
        _logger = logger;
        _workers = workers ?? Enumerable.Empty<ISyncService>();
    }

    // TODO: track last timestam p in DBs for update span to only update what is new
    public async Task<int> UpdateExentaStatuses(int minutes)
    {
        // load the milestones mappings
        _milestoneMappings = await _milestoneMapperRepository.GetAllMilestoneMappingsAsync();

        // get all data to update from shopfloor DBs
        List<UpdateData> data = await CollectData(minutes);
        if (data.Count == 0)
        {
            _logger.LogInformation("No Exenta status updates found.");
            return 0;
        }

        using var scope = _scopeFactory.CreateScope();
        var workerLogger = scope.ServiceProvider.GetRequiredService<ILogger<IUpdateExentaStatusesService>>();
        var tasks = _workers.Select(w => w.SyncData(data, _milestoneMappings, _scopeFactory, workerLogger));
        int[] results = await Task.WhenAll(tasks);
        return results.Sum();
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