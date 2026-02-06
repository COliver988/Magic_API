using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWW_Api.Models.Magic;
using MWW_Api.Repositories.Magic;

namespace MWW_MagicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DAPPartnersController : ControllerBase
{
    private readonly IDapPartnersRepository _dapPartnersRepository;

    public DAPPartnersController(IDapPartnersRepository dapPartnersRepository)
    {
        _dapPartnersRepository = dapPartnersRepository;
    }

    // GET: api/<DAPPartnersController>
    [HttpGet("ByPO")]
    public async Task<DapPartner?> GetByPO(string po)
    {
        return await _dapPartnersRepository.GetByPO(po);
    }

    [HttpGet("ByTKref1")]
    public async Task<DapPartner?> GetByTKRef1(string tkref1)
    {
        return await _dapPartnersRepository.GetByTKRef1(tkref1);
    }

    // note: po may be a list of POs
    [HttpGet("MoveOrder")]
    public async Task<ActionResult<List<string>>> MoveOrder(string po, string location)
    {
        var moved = await _dapPartnersRepository.MoveOrderAsync(po, location);

        if (moved == null || moved.Count == 0)
            return NoContent();

        // Explicitly return JSON (Ok serializes to application/json by default)
        return Ok(moved);
    }
}
