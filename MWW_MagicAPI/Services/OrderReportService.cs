using Microsoft.AspNetCore.Mvc;
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
    public async Task<List<OrdersByHourDTO>> GetByHour(int hours)
    {
        var now = DateTime.UtcNow;
        var result = _context.DapPartners.Where(p => p.DATE_PLACED >= now.AddHours(-hours) && p.TKRef2 != "JSON")
             .GroupBy(order => new { Date = order.DATE_PLACED.Date, Hour = order.DATE_PLACED.Hour })
             .Select(g => new OrdersByHourDTO
             {
                 Date = g.Key.Date,
                 Hour = g.Key.Hour,
                 Orders = g.Count(),
             })
             .OrderBy(o => o.Date).ThenBy(o => o.Hour)
             .ToList();
        return result;
    }
}