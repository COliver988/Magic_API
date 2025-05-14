using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;
public interface IMWW_ApplicationRepository
{
     Task<List<MWW_Applications>> GetActive();       
}