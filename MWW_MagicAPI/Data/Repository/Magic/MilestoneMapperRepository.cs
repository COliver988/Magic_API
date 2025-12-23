using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MWW_Api.Config;
using MWW_Api.Models.Magic;


namespace MWW_Api.Repositories.Magic;
public class MilestoneMapperRepository : IMilestoneMapperRepository
{
    private readonly MagicDbContext _context;
    private readonly IMemoryCache _cache;

    public MilestoneMapperRepository(MagicDbContext context,
        IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<MilestoneMapper> AddMilestoneMappingAsync(MilestoneMapper milestoneMapper)
    {
        _context.MilestoneMappers.Add(milestoneMapper);
        await _context.SaveChangesAsync();
        clearCache();
        return milestoneMapper;
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
        return cachedMappings;
    }

    public async Task<MilestoneMapper?> GetMilestoneMappingByIdAsync(int id) => await _context.MilestoneMappers.FindAsync(id);

    public async Task<MilestoneMapper> UpdateMilestoneMappingAsync(MilestoneMapper milestoneMapper)
    {
        _context.MilestoneMappers.Update(milestoneMapper);
        await  _context.SaveChangesAsync();
        clearCache();
        return milestoneMapper;
    }

    private void clearCache()
    {
        _cache.Remove("MilestoneMappings");
    }
}