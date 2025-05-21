using Microsoft.AspNetCore.Mvc;
using MWW_MagicAPI.Data.Models.DTO;
using MWW_MagicAPI.Services;

namespace MWW_MagicAPI.Http.Controllers.Authentication;
[Route("api/[controller]")]
public class AuthenticationController : Controller
{
    private readonly IAuthService _authService;

    public AuthenticationController( IAuthService authService)
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
        TokenResponse? token = await _authService.GenerateToken(user);
        if (token == null)
            return Unauthorized();
        var transformedResponse = new
        {
            access_token = token.access_token,
            token_type = token.token_type,
            expires_in = token.expires_in
        };
        return Ok(transformedResponse);
    }
}