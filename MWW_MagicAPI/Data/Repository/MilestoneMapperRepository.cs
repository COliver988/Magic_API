namespace MWW_Api.Repositories.Magic;

public class MilestoneMapperRepository : IMilestoneMappingRepository
{
    private readonly MagicDbContext _context;
    private readonly IMemoryCache _cache;

    public MilestoneMapperRepository(MagicDbContext context,
        ImemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<MilestoneMapper>> GetAllMilestoneMappingsAsync()
    {
        if (!_cache.TryGetValue("MilestoneMappings", out List<MilestoneMapper> cachedMappings))
        {
            cachedMappings = await _context.MilestoneMappers.ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(6));

            _cache.Set("MilestoneMappings", cachedMappings, cacheEntryOptions);
        }
        return await _context.MilestoneMappers.ToListAsync();
    }
}