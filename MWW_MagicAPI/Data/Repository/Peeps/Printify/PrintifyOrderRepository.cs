using Microsoft.EntityFrameworkCore;
using MWW_Api.Models.Peeps.Printify;
using MWW_MagicAPI.Data.Contexts;
using MWW_MagicAPI.Data.RepositoryContracts.Peeps.Printify;

namespace MWW_MagicAPI.Data.Repository.Peeps.Printify;

public class PrintifyOrderRepository : IPrintifyOrderRepository
{
    private readonly PeepsDbContext _context;
    private HttpClient _httpClient;
    private ILogger<PrintifyOrderRepository>? _logger;

    public PrintifyOrderRepository(PeepsDbContext context, HttpClient httpClient, ILogger<PrintifyOrderRepository>? logger)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PrintifyOrder?> GetByOrderPOAsync(string po) => await _context.PrintifyOrders.AsNoTracking().FirstOrDefaultAsync(o => o.UniqueId == po);

    /// <summary>
    /// update the order; will do any missing states in case we got them skipped
    /// </summary>
    /// <param name="order"></param>
    /// <param name="newStatus"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(string po, string newStatus)
    {
        // get it into tracked state
        PrintifyOrder? order = await _context.PrintifyOrders.FirstOrDefaultAsync(o => o.UniqueId == po);
        if (order == null || order.Status == newStatus) return false;

        return true;
    }
}
