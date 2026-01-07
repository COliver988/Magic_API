
using MWW_Api.Models.Magic;
using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services.SyncServices;

public class PrintifySyncService : ISyncService
{
    private IServiceScopeFactory _scopeFactory;
    private List<MilestoneMapper> _mappings;
    private ILogger<IUpdateExentaStatusesService> _logger;

    public async Task<int> SyncData(List<UpdateData> data, List<MilestoneMapper> milestoneMappings, IServiceScopeFactory scopeFactory, ILogger<IUpdateExentaStatusesService> logger)
    {
        _mappings = milestoneMappings;
        _scopeFactory = scopeFactory;
        _logger = logger;

        if (data == null || data.Count() == 0) return 0;
        return await UpdatePrintifyStatuses(data);
    }

    private async Task<int> UpdatePrintifyStatuses(List<UpdateData> updateDataList)
    {
        int updated = 0;
        foreach (UpdateData updateData in updateDataList)
        {
            MilestoneMapper? mapped = _mappings.FirstOrDefault(m => m.NewStatus == updateData.MilestoneName && !String.IsNullOrEmpty(m.FS_Status));
            if (mapped == null) continue;

            //TODO: add the event record for mapping.PrintifyStatus change
            //TODO: upate printify record status
            updated++;
        }

        return updated;
    }
}
