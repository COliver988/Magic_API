using Microsoft.AspNetCore.Mvc;
using MWW_Api.Repositories.Magic;

namespace MWW_MagicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UndefinedProductsController : Controller
{
    private readonly IUndefinedProductsRepository _repository;

    public UndefinedProductsController(IUndefinedProductsRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("UpsertUndefinedProdcut")]
    async public Task<IActionResult> UpsertUndefinedProduct(string customerId, string vendorPo, string productCode, int interfaceId)
    {
        try
        {
            bool results = await _repository.UpsertUndefinedProduct(customerId, productCode, vendorPo, interfaceId);
            if (results)
            {
                return Ok(new { message = "Product upserted successfully." });
            }
            else
            {
                return BadRequest(new { message = "Failed to upsert product." });
            }
        }
        catch (Exception ex)
        {
            // Log the exception (not implemented here)
            return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
        }
    }
}
