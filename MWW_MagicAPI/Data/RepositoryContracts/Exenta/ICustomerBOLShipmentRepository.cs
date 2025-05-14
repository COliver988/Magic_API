using MWW_Api.Models.Exenta;

namespace MWW_Api.Repositories.Exenta;

public interface ICustomerBOLShipmentRepository
{
    public CustomerBOLShipment? GetByVicsBol(string vicsbolno);
}
