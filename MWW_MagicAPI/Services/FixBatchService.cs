using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Shopfloor;

namespace MWW_MagicAPI.Services;

public class FixBatchService : IFixBatchService
{
    private readonly IShopfloorDbContextFactory _contextFactory;
    private readonly ExentaDbContext _exentaContext;

    public FixBatchService(IShopfloorDbContextFactory contextFactory, ExentaDbContext exentaContext)
    {
        _contextFactory = contextFactory;
        _exentaContext = exentaContext;
    }

    public async Task<List<Unit>> GetMissingBatches(string batchId)
    {
        var context = _contextFactory.GetContext(batchId);
        List<Unit> units = await context.Units
            .Where(u => u.BatchId == batchId)
            .ToListAsync();
        return units;
    }
}