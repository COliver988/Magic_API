using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services;

public interface IOrderReportService
{
    //Task<IQueryable<OrdersByHourDTO>> GetByHour(int hour);
    Task<List<OrdersByHourDTO>> GetByHour(int hour);
}