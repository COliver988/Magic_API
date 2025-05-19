using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public interface IStuckProductionOrderRepository
{
    Task<List<StuckProductionOrders>> GetAll(string? filter, int? pageNumber, int? pageSize);
    Task<int> GetCount();
}
