using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWW_Api.Models.Magic;
using MWW_Api.Repositories.Magic;

namespace MWW_MagicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WebItemController : ControllerBase
{
    private readonly IWebItemRepository _repo;

    public WebItemController(IWebItemRepository repo)
    {
        _repo = repo;
    }  

    // GET: api/<DAPPartnersController>
    [HttpGet("FindItem/{itemCode}")]
    public async Task<WebItem?> FindItem(string itemCode)
    {
        return await _repo.GetByItemCodeAsync(itemCode);
    }
}
