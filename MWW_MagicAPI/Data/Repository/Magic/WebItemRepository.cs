using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public class WebItemRepository : IWebItemRepository
{
    private readonly MagicDbContext _context;

    public WebItemRepository(MagicDbContext magicApi)
    {
        _context = magicApi;
    }

    public async Task<WebItem?> GetByItemCodeAsync(string itemCode) =>
        await _context.WebItems.FirstOrDefaultAsync(wi => wi.Item_code == itemCode);
}
