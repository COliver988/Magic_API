using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWW_Api.Models.Magic;
using MWW_Api.Repositories.Magic;

namespace MWW_MagicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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

    [HttpGet]
    public async Task<List<ProductOverride>> GetAll()
    {
        return await _repo.GetAll();
    }

    [HttpGet("ById")]
    public async Task<ProductOverride> GetById(long id)
    {
        return await _repo.GetByIdAsync(id);
    }

    [HttpPost("Update")]
    public async Task<ProductOverride> Update([FromBody] ProductOverride productOverride)
    {
        if (productOverride == null)
        {
            throw new ArgumentNullException(nameof(productOverride), "Product override cannot be null");
        }
        return await _repo.Update(productOverride);
    }
}
