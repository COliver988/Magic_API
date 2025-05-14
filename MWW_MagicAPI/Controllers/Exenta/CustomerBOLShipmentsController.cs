using Microsoft.AspNetCore.Mvc;
using MWW_Api.Models.Exenta;
using MWW_Api.Repositories.Exenta;

namespace MWW_MagicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerBOLShipmentsController : ControllerBase
{
    private readonly ICustomerBOLShipmentRepository _repo;

    public CustomerBOLShipmentsController(ICustomerBOLShipmentRepository repo)
    {
        _repo = repo;
    }  

    [HttpGet("ByVicsBol")]
    public async Task<CustomerBOLShipment?> GetByVicsBol(string vicsbolno)
    {
        return await _repo.GetByVicsBol(vicsbolno);
    }
}
