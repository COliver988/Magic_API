using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;
public class ProductOverrideRepository : IProductOverrideRepository
{
    private readonly MagicDbContext _context;

    public ProductOverrideRepository(MagicDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductOverride>> GetAllByOverrideType(int overrideType) => await _context.ProductOverrides.Where(p => p.OverrideType == overrideType).ToListAsync();
    public async Task<ProductOverride?> GetByProductAndOverrideType(string product, int overrideType) => await _context.ProductOverrides.Where(p => p.OverrideType == overrideType && p.ProductCode == product).FirstOrDefaultAsync();
}