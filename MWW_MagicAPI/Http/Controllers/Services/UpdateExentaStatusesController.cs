using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWW_MagicAPI.Services;

namespace MWW_MagicAPI.Http.Controllers.Services;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UpdateExentaStatusesController : Controller
{
    private readonly IUpdateExentaStatusesService _updateExentaStatusesService;
    public IActionResult UpdateExentStatuses(int minutes)
    {
        try
        {
            bool result = _updateExentaStatusesService.UpdateExentaStatuses(minutes);
            if (result)
            {
                return Ok(new { Message = "Exenta statuses updated successfully." });
            }
            else
            {
                return StatusCode(500, new { Message = "Failed to update Exenta statuses." });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while updating Exenta statuses.", Details = ex.Message });
        }
    }
}
