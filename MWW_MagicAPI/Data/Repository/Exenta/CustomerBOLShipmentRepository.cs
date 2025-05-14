using MWW_Api.Config;
using MWW_Api.Models.Exenta;

namespace MWW_Api.Repositories.Exenta;

public class CustomerBOLShipmentRepository : ICustomerBOLShipmentRepository
{

    private readonly ExentaDbContext _context;

    public CustomerBOLShipmentRepository(ExentaDbContext context)
    {
        _context = context;
    }
    
    public CustomerBOLShipment? GetByVicsBol(string vicsbolno) =>  _context.CustomerBOLShipments.Where(c => c.VICSBOLNO == vicsbolno).FirstOrDefault();
}
