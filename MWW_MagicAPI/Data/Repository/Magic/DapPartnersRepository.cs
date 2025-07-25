using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public class DapPartnersRepository : IDapPartnersRepository
{
    private readonly MagicDbContext _context;

    public DapPartnersRepository(MagicDbContext context)
    {
        _context = context;
    }

    public async Task<DapPartner?> GetByPO(string po) => await _context.DapPartners.Where(d => d.PO == po).FirstOrDefaultAsync();

    public async Task<DapPartner?> GetByTKRef1(string po) => await _context.DapPartners.Where(d => d.TKRef1 == po).FirstOrDefaultAsync();
    public async Task<DapPartner?> MoveOrderAsync(string po, string location)
    {
        // Step 1: Get the existing record to fetch custid (CC_APPROVED)
        var partner = await _context.DapPartners
            .FirstOrDefaultAsync(d => d.PO == po);

        if (partner == null || string.IsNullOrWhiteSpace(partner.CC_APPROVED))
        {
            // No matching record or no customer ID to work with
            return null;
        }

        var custId = partner.CC_APPROVED;
        var TKRef1 = partner.TKRef1;

        // Step 2: Call stored procedure with all required parameters
        var sql = "EXEC dbo.util_MoveOrderLocation @orderID = @p0, @custid = @p1, @MoveLocation = @p2";
        await _context.Database.ExecuteSqlRawAsync(sql, TKRef1, custId, location);

        // Step 3: Return the updated partner record (optional)
        return await _context.DapPartners
            .FirstOrDefaultAsync(d => d.PO == po && d.CC_APPROVED == custId);
    }

}
