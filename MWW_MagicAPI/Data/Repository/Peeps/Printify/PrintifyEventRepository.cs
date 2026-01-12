using Microsoft.EntityFrameworkCore;
using MWW_Api.Models.Peeps.Printify;
using MWW_MagicAPI.Data.Contexts;
using MWW_MagicAPI.Data.RepositoryContracts.Peeps.Printify;

namespace MWW_MagicAPI.Data.Repository.Peeps.Printify;

public class PrintifyEventRepository : IPrintifyEventRepository
{
    private readonly PeepsDbContext _context;
    private readonly ILogger<PrintifyEventRepository> _logger;

    public PrintifyEventRepository(PeepsDbContext context, ILogger<PrintifyEventRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<PrintifyEvent>> AddEvents(List<PrintifyEvent> printifyEvents)
    {
        _context.PrintifyEvents.AddRange(printifyEvents);
        await _context.SaveChangesAsync();
        return printifyEvents;
    }

    public async Task<List<PrintifyEvent>> GetAllByOrder(long orderId) => await _context.PrintifyEvents
        .Where(e => e.OrderId == orderId)
        .AsNoTracking()
        .ToListAsync();
}
