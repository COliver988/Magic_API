using Microsoft.AspNetCore.Mvc;
using MWW_MagicAPI.Services;

namespace MWW_MagicAPI.Http.Controllers.Reports
{
    [Route("[controller]")]
    public class OrderReportsController : Controller
    {
        IOrderReportService _orderReportService;
        private readonly ILogger<OrderReportsController> _logger;

        public OrderReportsController(ILogger<OrderReportsController> logger,
            IOrderReportService orderReportService)
        {
            _orderReportService = orderReportService;
            _logger = logger;
        }

        [HttpGet("ByHour")]
        public async Task<IActionResult> GetByHour(int hour)
        {
            try
            {
                await _orderReportService.GetByHour(hour);
                return Ok(); // Return appropriate response based on your service logic
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders by hour");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}