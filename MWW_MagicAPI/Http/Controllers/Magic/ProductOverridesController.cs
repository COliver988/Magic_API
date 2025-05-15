using Microsoft.AspNetCore.Mvc;
using MWW_Api.Models.Magic;
using MWW_Api.Repositories.Magic;

namespace MWW_MagicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductOverridesController : ControllerBase
{
    private readonly IProductOverrideRepository _repo;

    public ProductOverridesController(IProductOverrideRepository repo)
    {
        _repo = repo;
    }  

    [HttpGet("AllByOverrideType")]
    public async Task<List<ProductOverride>> GetAllByOverrideType(int overrideType)
    {
        return await _repo.GetAllByOverrideType(overrideType);
    }

    [HttpGet("ByProductAndOverrideType")]
    public async Task<ProductOverride?> GetByProductAndOverrideType(string product, int overrideType)
    {
        return await _repo.GetByProductAndOverrideType(product, overrideType);
    }
}
