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
        var orderID = po;
        var custId = ""; 

        var sql = "EXEC dbo.util_MoveOrderLocation @orderID = {0}, @custid = {1}, @MoveLocation = {2}";
        await _context.Database.ExecuteSqlRawAsync(sql, orderID, custId, location);

        return await _context.DapPartners
            .FirstOrDefaultAsync(d => d.TKRef1 == orderID && d.CC_APPROVED == custId);
    }

}
