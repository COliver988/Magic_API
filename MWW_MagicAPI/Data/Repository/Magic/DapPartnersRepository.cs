using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public class DapPartnersRepository : IDapPartnersRepository
{
    private readonly MagicDbContext _context;
    private readonly IServiceScopeFactory _scopeFactory;

    public DapPartnersRepository(MagicDbContext context, IServiceScopeFactory scopeFactory)
    {
        _context = context;
        _scopeFactory = scopeFactory;
    }

    public async Task<DapPartner?> GetByPO(string po) => await _context.DapPartners.Where(d => d.PO == po).FirstOrDefaultAsync();

    public async Task<DapPartner?> GetByTKRef1(string po) => await _context.DapPartners.Where(d => d.TKRef1 == po).FirstOrDefaultAsync();

    /// <summary>
    /// update the PO location; note that PO may be a list of POs
    /// runs up to 5 MoveSingleOrderAsync calls in parallel; each call uses its own scoped DbContext.
    /// </summary>
    /// <param name="po">one or more POs</param>
    /// <param name="location">location to move to</param>
    /// <returns>the first updated DapPartner or null</returns>
    public async Task<List<string>>? MoveOrderAsync(string po, string location)
    {
        List<string> poList = po.Split(',', ' ').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList();
        var semaphore = new SemaphoreSlim(5);
        var tasks = poList.Select(async singlePo =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await MoveSingleOrderAsync(singlePo, location);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        string?[] results = await Task.WhenAll(tasks);
        List<string> updatedPOs = results.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r!).ToList();

        return updatedPOs;
    }

    private async Task<string?> MoveSingleOrderAsync(string po, string location)
    {
        // Use a new scoped MagicDbContext for thread-safety when running in parallel.
        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MagicDbContext>();

        // Step 1: Get the existing record to fetch custid (CC_APPROVED)
        var partner = await ctx.DapPartners.FirstOrDefaultAsync(d => d.PO == po);

        if (partner == null || string.IsNullOrWhiteSpace(partner.CC_APPROVED))
        {
            // No matching record or no customer ID to work with
            return null;
        }

        var custId = partner.CC_APPROVED;
        var TKRef1 = partner.TKRef1;

        // Step 2: Call stored procedure with all required parameters
        var sql = "EXEC dbo.util_MoveOrderLocation @orderID = @p0, @custid = @p1, @MoveLocation = @p2";
        await ctx.Database.ExecuteSqlRawAsync(sql, TKRef1, custId, location);

        // Step 3: Return the PO if found; note if not found then no update occurred
        DapPartner? dapPartner = await ctx.DapPartners
            .FirstOrDefaultAsync(d => d.PO == po && d.CC_APPROVED == custId);
        return dapPartner?.PO;
    }

}