using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public class StuckProductionOrderRepository : IStuckProductionOrderRepository
{
    private readonly MagicDbContext _context;

    public StuckProductionOrderRepository(MagicDbContext context)
    {
        _context = context;
    }

    public async Task<List<StuckProductionOrders>> GetAll(string? filter, int? pageNumber, int? pageSize)
    {
        if (filter != null)
            return await _context.StuckProductionOrders.AsNoTracking().Where(x => x.PO.Contains(filter)).ToListAsync();

        if (pageNumber == null) pageNumber = 1;
        if (pageSize == null) pageSize = 20;
        return await _context.StuckProductionOrders
          .AsNoTracking()
          .Skip((pageNumber.Value - 1) * pageSize.Value)
          .Take(pageSize.Value)
          .ToListAsync();
    }

    public async Task<int> GetCount() => await _context.StuckProductionOrders.AsNoTracking().CountAsync();
}
