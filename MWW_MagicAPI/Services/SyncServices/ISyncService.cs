using MWW_Api.Models.Magic;
using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services.SyncServices;

public interface ISyncService
{
    Task<int> SyncData(List<UpdateData> data, List<MilestoneMapper> milestoneMappings, IServiceScopeFactory scopeFactory, ILogger<IUpdateExentaStatusesService> logger);
}
