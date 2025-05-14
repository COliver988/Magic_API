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

    public OrderHeader? GetByPONum(string po) => _context.OrderHeaders.Where(o => o.PONUM == po).FirstOrDefault();
}
