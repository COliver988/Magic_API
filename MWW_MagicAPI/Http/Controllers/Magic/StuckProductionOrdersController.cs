using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWW_Api.Models.Magic;
using MWW_Api.Repositories.Magic;

namespace MWW_MagicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StuckProductionOrdersController : ControllerBase
{
    private readonly IStuckProductionOrderRepository _repo;

    public StuckProductionOrdersController(IStuckProductionOrderRepository repo)
    {
        _repo = repo;
    }  

    [HttpGet("All")]
    public async Task<List<StuckProductionOrders>> GetAll(string? filter, int? pageNumber, int? pageSize)
    {
        return await _repo.GetAll(filter, pageNumber, pageSize);
    }

    [HttpGet("Count")]
    public async Task<int?> GetCount()
    {
        return await _repo.GetCount();
    }
}
