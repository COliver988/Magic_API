using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services.SyncServices;
public interface IUpdateExentaStatusesService
{
    Task<List<SyncDataResults>> UpdateExentaStatuses(int minutes);
}