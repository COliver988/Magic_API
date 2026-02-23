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

        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MagicDbContext>();

        // 1. Try to find existing
        var existing = await ctx.SFCTimestamps.FirstOrDefaultAsync(s => s.Location == sFCTimestamp.Location);

        if (existing != null)
        {
            existing.LastChecked = sFCTimestamp.LastChecked;
            ctx.SFCTimestamps.Update(existing);
        }
        else
        {
            // 2. Add as new
            ctx.SFCTimestamps.Add(sFCTimestamp);
        }

        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // 3. RACE CONDITION HANDLER: 
            // If another parallel thread inserted this location between our 'find' and 'save'
            var concurrentRecord = await ctx.SFCTimestamps
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Location == sFCTimestamp.Location);

            if (concurrentRecord != null)
            {
                concurrentRecord.LastChecked = sFCTimestamp.LastChecked;
                ctx.SFCTimestamps.Update(concurrentRecord);
                await ctx.SaveChangesAsync();
                return concurrentRecord;
            }
            throw; // Re-throw if it was a different DB error
        }

        return existing ?? sFCTimestamp;
    }
}