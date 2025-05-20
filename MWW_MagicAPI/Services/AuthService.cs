using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MWW_Api.Models.Magic;
using MWW_Api.Repositories.Magic;
using MWW_MagicAPI.Data.Models;
using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services;

public class AuthService  : IAuthService
{
    private readonly AuthSettings _settings;
    private readonly IMWW_ApplicationRepository _mww_ApplicationRepository;

    public AuthService(IOptions<AuthSettings> settings,
        IMWW_ApplicationRepository mWW_ApplicationRepository)
    {
        _settings = settings.Value;
        _mww_ApplicationRepository = mWW_ApplicationRepository;
    }

    public async Task<string> GenerateToken(AuthenticationUser user)
    {
        bool validUser = await validateUser(user);
        if (!validUser)
            return string.Empty;

        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_settings.PrivateKey);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = GenerateClaims(user),
            Expires = DateTime.UtcNow.AddMinutes(_settings.Timeout),
            SigningCredentials = credentials,
        };

        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    // need to validate this user; may be an API to API (MWW_Applications) or user (WebAPI_Customer - future)
    private async Task<bool> validateUser(AuthenticationUser user)
    {
        MWW_Applications? mWW_Applications = await _mww_ApplicationRepository.GetByName(user.Name);
        if (mWW_Applications == null || !mWW_Applications.Active)
            return false;
        return true;
    }

    private static ClaimsIdentity GenerateClaims(AuthenticationUser user)
    {
        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.Name, user.Name));

        return claims;
    }
}