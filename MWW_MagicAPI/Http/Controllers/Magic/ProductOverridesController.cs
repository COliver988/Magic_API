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
    public async Task<List<ProductOverride>> GetAllByOverrideType(int? overrideType)
    {
        return await _repo.GetAllByOverrideType(overrideType);
    }

    [HttpGet("ByProductAndOverrideType")]
    public async Task<ProductOverride?> GetByProductAndOverrideType(string product, int overrideType)
    {
        return await _repo.GetByProductAndOverrideType(product, overrideType);
    }

    [HttpGet("ByProductCode")]
    public async Task<List<ProductOverride>> GetByProduct(string productCode)
    {
        return await _repo.GetByProduct(productCode);
    }

    [HttpGet("ById")]
    public async Task<ProductOverride> GetById(int id)
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

    [HttpGet("Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        ProductOverride? productOverride = await _repo.GetByIdAsync(id);
        if (productOverride == null)
        {
            return NotFound($"Product override with ID {id} not found.");
        }
        try
        {
            await _repo.Delete(productOverride);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
