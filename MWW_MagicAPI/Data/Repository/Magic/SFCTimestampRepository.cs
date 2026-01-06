using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public class SFCTimestampRepository : ISFCTimestampRepository
{
    private MagicDbContext _context;
    private readonly IServiceScopeFactory _scopeFactory;

    public SFCTimestampRepository(MagicDbContext context, IServiceScopeFactory scopeFactory)
    {
        _context = context;
        _scopeFactory = scopeFactory;
    }

    public async Task<List<SFCTimestamp>> GetAllAsync() => await _context.SFCTimestamps.AsNoTracking().ToListAsync();

    public async Task<SFCTimestamp?> GetByLocationAsync(string location) => await _context.SFCTimestamps.FirstOrDefaultAsync(s => s.Location == location);

    /// <summary>
    /// updates or adds a new record using a short‑lived DbContext so concurrent callers do not share the same context instance.
    /// </summary>
    /// <param name="sFCTimestamp"></param>
    /// <returns>updated/added record</returns>
    public async Task<SFCTimestamp> UpdateAsync(SFCTimestamp sFCTimestamp)
    {
        if (sFCTimestamp == null) throw new ArgumentNullException(nameof(sFCTimestamp));
        if (string.IsNullOrWhiteSpace(sFCTimestamp.Location)) throw new ArgumentException("Location is required", nameof(sFCTimestamp));

        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MagicDbContext>();

        // attempt to find existing record in new scoped context
        SFCTimestamp? existing = await ctx.SFCTimestamps.FirstOrDefaultAsync(s => s.Location == sFCTimestamp.Location);
        if (existing != null)
        {
            existing.LastChecked = DateTime.UtcNow;
            ctx.SFCTimestamps.Update(existing);
            await ctx.SaveChangesAsync();
            return existing;
        }
        else
        {
            sFCTimestamp.LastChecked = DateTime.UtcNow;
            ctx.SFCTimestamps.Add(sFCTimestamp);
            await ctx.SaveChangesAsync();
            return sFCTimestamp;
        }
    }
}