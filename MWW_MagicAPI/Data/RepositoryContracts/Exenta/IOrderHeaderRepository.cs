using  MWW_Api.Models.Exenta;

namespace MWW_Api.Repositories.Exenta;

public interface IOrderHeaderRepository
{
    public OrderHeader? GetByPONum(string po);
}
