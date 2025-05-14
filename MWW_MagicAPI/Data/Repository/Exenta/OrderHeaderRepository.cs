using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Exenta;

namespace MWW_Api.Repositories.Exenta;

public class OrderHeaderRepository : IOrderHeaderRepository
{
    private readonly ExentaDbContext _context;

    public OrderHeaderRepository(ExentaDbContext context)
    {
        _context = context;
    }

    public async Task<OrderHeader?> GetByPONum(string po) => await _context.OrderHeaders.Where(o => o.PONUM == po).FirstOrDefaultAsync();
}
