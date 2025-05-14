using  MWW_Api.Models.Exenta;

namespace MWW_Api.Repositories.Exenta;

public interface IOrderHeaderRepository
{
    public Task<OrderHeader?> GetByPONum(string po);
}
