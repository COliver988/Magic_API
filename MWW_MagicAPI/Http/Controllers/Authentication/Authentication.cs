using Microsoft.AspNetCore.Mvc;
using MWW_MagicAPI.Data.Models.DTO;
using MWW_MagicAPI.Services;

namespace MWW_MagicAPI.Http.Controllers.Authentication;
[Route("[controller]")]
public class Authentication : Controller
{
    private readonly IAuthService _authService;

    public Authentication( IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("API")]
    public async Task<IActionResult> AuthenticateAPI([FromBody] AuthenticationUser user)
    {
        if (user == null)
        {
            return BadRequest("Invalid client request");
        }
        string? token = await _authService.GenerateToken(user);
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized();
        }
        return Ok(new { Token = token });
    }
}