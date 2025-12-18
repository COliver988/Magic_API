using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWW_Api.Models.Magic;
using MWW_Api.Repositories.Magic;

namespace MWW_MagicAPI.Controllers;

[Route("api/[controller]")]
//[Authorize]
[ApiController]
public class MilestoneMapperController : Controller
{
    private readonly IMilestoneMapperRepository _repository;

    public MilestoneMapperController(IMilestoneMapperRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("All")]
    public async Task<IActionResult> GetAllMilestoneMapper()
    {
        return Ok(await _repository.GetAllMilestoneMappingsAsync());
    }

    [HttpGet("Load")]
    public async Task<IActionResult> GetMilestoneMapper(int id)
    {
        MilestoneMapper? mapper = await _repository.GetMilestoneMappingByIdAsync(id);
        if (mapper == null)
            return NotFound(new { message = "MilestoneMapper not found." });
        else
            return Ok(await _repository.GetMilestoneMappingByIdAsync(id));
    }

    [HttpPost("Add")]
    public async Task<IActionResult> AddMilestoneMapper(MilestoneMapper mapper)
    {
        try
        {
            MilestoneMapper results = await _repository.AddMilestoneMappingAsync(mapper);
            if (results != null)
            {
                return Ok(new { message = "MilestoneMapper added successfully." });
            }
            else
            {
                return BadRequest(new { message = "Failed to MilestoneMapper." });
            }
        }
        catch (Exception ex)
        {
            // Log the exception (not implemented here)
            return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
        }
    }

    [HttpPost("Update")]
    public async Task<IActionResult> UpdateMilestoneMapper(MilestoneMapper mapper)
    {
        try
        {
            MilestoneMapper results = await _repository.UpdateMilestoneMappingAsync(mapper);
            if (results != null)
            {
                return Ok(new { message = "MilestoneMapper updated successfully." });
            }
            else
            {
                return BadRequest(new { message = "Failed to MilestoneMapper." });
            }
        }
        catch (Exception ex)
        {
            // Log the exception (not implemented here)
            return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
        }
    }
}
