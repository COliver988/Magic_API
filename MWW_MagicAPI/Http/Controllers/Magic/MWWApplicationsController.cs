using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWW_Api.Models.Magic;
using MWW_Api.Repositories.Magic;

namespace MWW_MagicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MWWApplicationsController : ControllerBase
{
    private readonly IMWW_ApplicationRepository _applicationRepository;

    public MWWApplicationsController(IMWW_ApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }  

    // GET: api/<DAPPartnersController>
    [HttpGet("Active")]
    public async Task<List<MWW_Applications>> GetActive()
    {
        return await _applicationRepository.GetActive();
    }
}
