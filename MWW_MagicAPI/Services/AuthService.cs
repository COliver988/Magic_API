using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthCore.Helpers;
using AuthCore.Models;
using Microsoft.IdentityModel.Tokens;
using MWW_Api.Models.Magic;
using MWW_MagicAPI.Data.Models;

namespace MWW_MagicAPI.Services;

public class AuthService  : IAuthService
{
    private readonly AuthSettings _settings;

    public AuthService(AuthSettings settings)
    {
        _settings = settings;
    }
    public string GenerateToken(WebAPI_Customer user)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_settings.AuthKey);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = GenerateClaims(user),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = credentials,
        };

        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    private static ClaimsIdentity GenerateClaims(WebAPI_Customer user)
    {
        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.Name, user.Email));

        return claims;
    }
}