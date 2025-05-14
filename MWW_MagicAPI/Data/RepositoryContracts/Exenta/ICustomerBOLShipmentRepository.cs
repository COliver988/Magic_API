using MWW_Api.Models.Exenta;

namespace MWW_Api.Repositories.Exenta;

public interface ICustomerBOLShipmentRepository
{
    public Task<CustomerBOLShipment?> GetByVicsBol(string vicsbolno);
}
