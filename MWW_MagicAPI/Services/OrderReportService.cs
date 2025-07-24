using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services;


public class OrderReportService : IOrderReportService
{
        
    private readonly MagicDbContext _context;

    public OrderReportService(MagicDbContext context)
    {
        _context = context;
    }

    //public async Task<IQueryable<OrdersByHourDTO>> GetByHour(int hour)
    public async Task GetByHour(int hour)
    {
        var query = _context.DapPartners.AsQueryable();

        var now = DateTime.UtcNow;
        query.Where(p => p.DATE_PLACED >= now.AddHours(-hour))
             .GroupBy(order => new { Date = order.DATE_PLACED.Date, Hour = order.DATE_PLACED.Hour, PO = order.TKRef1 })
             .Select(g => new OrdersByHourDTO
             {
                 Date = g.Key.Date,
                 Hour = g.Key.Hour,
                 Orders = g.Count(),
             });
        var result = await query.ToListAsync();
        var hello = "hello";
    }
}