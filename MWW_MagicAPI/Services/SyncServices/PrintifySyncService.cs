using MWW_Api.Models.Magic;
using MWW_MagicAPI.Data.Models.DTO;
using MWW_MagicAPI.Data.RepositoryContracts.Peeps.Printify;
using MWW_MagicAPI.Services.SyncServices;

public class PrintifySyncService : ISyncService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PrintifySyncService> _logger;

    public PrintifySyncService(
        IServiceScopeFactory scopeFactory,
        ILogger<PrintifySyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<int> SyncData(
        List<UpdateData> data,
        List<MilestoneMapper> milestoneMappings)
    {
        if (data == null || data.Count == 0)
            return 0;

        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPrintifyOrderRepository>();

        return await UpdatePrintifyStatuses(data, milestoneMappings, repo);
    }

    private async Task<int> UpdatePrintifyStatuses(
        List<UpdateData> updates,
        List<MilestoneMapper> mappings,
        IPrintifyOrderRepository repo)
    {
        int updated = 0;

        foreach (var update in updates)
        {
            var mapped = mappings.FirstOrDefault(m =>
                m.NewStatus == update.MilestoneName &&
                !string.IsNullOrEmpty(m.PrintifyStatus));

            if (mapped == null)
                continue;

            if (await repo.UpdateAsync(update.SerialNumber, mapped.PrintifyStatus))
                updated++;
        }

        return updated;
    }
}
