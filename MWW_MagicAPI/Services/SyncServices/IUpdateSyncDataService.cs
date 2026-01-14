using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services.SyncServices;

public interface IUpdateSyncDataService
{
    public Task<List<UpdateData>> UpdateSyncData(List<UpdateData> data);
}
