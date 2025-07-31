using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public interface IDapPartnersRepository
{
    Task<DapPartner?> GetByTKRef1(string po);
    Task<DapPartner?> GetByPO(string po);
    Task<DapPartner?> MoveOrderAsync(string po, string location);
}
