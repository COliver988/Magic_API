using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Repositories.Magic;
using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services.SyncServices;

public class UpdateSyncDataService : IUpdateSyncDataService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UpdateSyncDataService(IDapPartnersRepository partnersRepository, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<List<UpdateData>> UpdateSyncData(List<UpdateData> data)
    {
        if (data == null || data.Count == 0)
            return data;

        // collect distinct, non-empty serial numbers
        var serials = data
            .Select(d => d.SerialNumber?.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (serials.Count == 0)
            return data;

        // perform batched queries against the DB to get PO -> TKRef1 mapping
        var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        const int chunkSize = 100;

        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MagicDbContext>();

        for (int i = 0; i < serials.Count; i += chunkSize)
        {
            var chunk = serials.Skip(i).Take(chunkSize).ToList();
            var rows = await ctx.DapPartners
                .AsNoTracking()
                .Where(d => chunk.Contains(d.PO))
                .Select(d => new { d.PO, d.TKRef1, d.FS_Status, d.FS_TrackingNumber })
                .ToListAsync();

            foreach (var r in rows)
            {
                if (!string.IsNullOrWhiteSpace(r.PO) && !mapping.ContainsKey(r.PO))
                {
                    mapping[r.PO] = r.TKRef1 ?? string.Empty;
                    mapping[$"FS_Status_{r.PO}"] = r.FS_Status ?? string.Empty;
                    mapping[$"FS_TrackingNumber_{r.PO}"] = r.FS_TrackingNumber ?? string.Empty;
                }
            }
        }

        // apply the mapping back to the input list
        foreach (var item in data)
        {
            if (!string.IsNullOrWhiteSpace(item.SerialNumber))
            {
                var serialKey = item.SerialNumber.Trim();
                if (mapping.TryGetValue(serialKey, out var tk))
                {
                    item.VendorPO = tk;
                }
                if (mapping.TryGetValue($"FS_Status_{serialKey}", out var fsStatus))
                {
                    item.FS_Status = fsStatus;
                }
                if (mapping.TryGetValue($"FS_TrackingNumber_{serialKey}", out var fsTrackingNumber))
                {
                    item.TrackingInfo = fsTrackingNumber;
                }
            }
        }

        return data;
    }
}
