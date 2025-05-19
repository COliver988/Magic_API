using MWW_Api.Models.Magic;

namespace MWW_MagicAPI.Services;
public interface IAuthService
{
     string GenerateToken(WebAPI_Customer user);       
}