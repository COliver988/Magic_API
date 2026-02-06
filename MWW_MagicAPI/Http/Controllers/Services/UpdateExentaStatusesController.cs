using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWW_MagicAPI.Data.Models.DTO;
using MWW_MagicAPI.Services.SyncServices;

namespace MWW_MagicAPI.Http.Controllers.Services;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public class UpdateExentaStatusesController : Controller
{
    private readonly IUpdateExentaStatusesService _updateExentaStatusesService;

    public UpdateExentaStatusesController(IUpdateExentaStatusesService updateExentaStatusesService)
    {
        _updateExentaStatusesService = updateExentaStatusesService;
    }

    [HttpGet("UpdateExentaStatuses/{minutes}")]
    public async Task<IActionResult> UpdateExentStatuses(int minutes)
    {
        try
        {
            List<SyncDataResults> result = await _updateExentaStatusesService.UpdateExentaStatuses(minutes);
            if (result.Count >= 0)
                return Ok(new { Message = $"Exenta statuses updated successfully.", Data = result });
            else
                return StatusCode(500, new { Message = "Failed to update Exenta statuses." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while updating Exenta statuses.", Details = ex.Message });
        }
    }
}
