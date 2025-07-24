using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services;

public interface IOrderReportService
{
    //Task<IQueryable<OrdersByHourDTO>> GetByHour(int hour);
    Task GetByHour(int hour);
}