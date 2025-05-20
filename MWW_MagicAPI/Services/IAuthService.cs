using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services;
public interface IAuthService
{
     Task<string?> GenerateToken(AuthenticationUser user);       
}