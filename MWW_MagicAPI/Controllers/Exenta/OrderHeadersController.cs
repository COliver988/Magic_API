using Microsoft.AspNetCore.Mvc;
using MWW_Api.Models.Exenta;
using MWW_Api.Repositories.Exenta;

namespace MWW_MagicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderHeadersController : ControllerBase
{
    private readonly IOrderHeaderRepository _repo;

    public OrderHeadersController(IOrderHeaderRepository repo)
    {
        _repo = repo;
    }  

    [HttpGet("ByPONum")]
    public async Task<OrderHeader?> GetByPONum(string po)
    {
        return await _repo.GetByPONum(po);
    }
}
