using Microsoft.EntityFrameworkCore;
using MWW_Api.Models.Peeps.Printify;
using MWW_MagicAPI.Data.Contexts;
using MWW_MagicAPI.Data.RepositoryContracts.Peeps.Printify;

namespace MWW_MagicAPI.Data.Repository.Peeps.Printify;

public class PrintifyOrderRepository : IPrintifyOrderRepository
{
    private readonly PeepsDbContext _context;
    private readonly ILogger<PrintifyOrderRepository> _logger;

    public PrintifyOrderRepository(PeepsDbContext context, ILogger<PrintifyOrderRepository>? logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PrintifyOrder?> GetByOrderPOAsync(string po) =>
        await _context.PrintifyOrders
            .AsNoTracking()
            .Include(o => o.PrintifyItems)
            .FirstOrDefaultAsync(o => o.UniqueId == po);

    /// <summary>
    /// update the order and its items
    /// </summary>
    /// <param name="order"></param>
    /// <param name="newStatus"></param>
    /// <returns>true if success else false</returns>
    public async Task<bool> UpdateAsync(string po, string newStatus)
    {
        // get it into tracked state
        PrintifyOrder? order = await _context.PrintifyOrders.FirstOrDefaultAsync(o => o.UniqueId == po);
        if (order == null || order.Status == newStatus) return false;

        order.Status = newStatus;
        List<PrintifyItem> printifyItems = await _context.PrintifyItems.Where(i => i.OrderId == order.Id).ToListAsync();
        foreach (var item in printifyItems)
            item.Status = newStatus;
        await _context.SaveChangesAsync();

        return true;
    }
}
