
using MWW_Api.Models.Magic;
using MWW_MagicAPI.Data.Models.DTO;
using MWW_MagicAPI.Data.RepositoryContracts.Peeps.Printify;

namespace MWW_MagicAPI.Services.SyncServices;

public class PrintifySyncService : ISyncService
{
    private IServiceScopeFactory _scopeFactory;
    private IPrintifyOrderRepository _printifyOrderRepository;
    private List<MilestoneMapper> _mappings;
    private ILogger<IUpdateExentaStatusesService> _logger;

    public async Task<int> SyncData(List<UpdateData> data, List<MilestoneMapper> milestoneMappings, IServiceScopeFactory scopeFactory, ILogger<IUpdateExentaStatusesService> logger)
    {
        _mappings = milestoneMappings;
        _scopeFactory = scopeFactory;
        _logger = logger;

        if (data == null || data.Count() == 0) return 0;
        using var scope = _scopeFactory.CreateScope();
        _printifyOrderRepository = scope.ServiceProvider.GetRequiredService<IPrintifyOrderRepository>();

        return await UpdatePrintifyStatuses(data);
    }

    private async Task<int> UpdatePrintifyStatuses(List<UpdateData> updateDataList)
    {
        int updated = 0;
        foreach (UpdateData updateData in updateDataList)
        {
            MilestoneMapper? mapped = _mappings.FirstOrDefault(m => m.NewStatus == updateData.MilestoneName && !String.IsNullOrEmpty(m.PrintifyStatus));
            if (mapped == null) continue;
            if (await _printifyOrderRepository.UpdateAsync(updateData.SerialNumber, mapped.PrintifyStatus))
                updated++;
        }

        return updated;
    }
}
