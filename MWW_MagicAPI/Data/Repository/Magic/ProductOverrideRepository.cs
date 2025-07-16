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

    public Task Delete(ProductOverride productOverride)
    {
        if (productOverride == null)
        {
            throw new ArgumentNullException(nameof(productOverride), "Product override cannot be null");
        }

        _context.ProductOverrides.Remove(productOverride);
        return _context.SaveChangesAsync();
    }

    public async Task<List<ProductOverride>> GetAllByOverrideType(int? overrideType)
    {
        if (overrideType == 0)
            return await _context.ProductOverrides.ToListAsync();
        else
            return await _context.ProductOverrides.Where(p => p.OverrideType == overrideType).ToListAsync();
    }

    public async Task<List<ProductOverride>> GetByProduct(string product) => await _context.ProductOverrides.Where(p => p.ProductCode == product).ToListAsync();

    public Task<ProductOverride>? GetByIdAsync(int id) => _context.ProductOverrides.Where(p => p.Id == id).FirstOrDefaultAsync();

    public async Task<ProductOverride?> GetByProductAndOverrideType(string product, int overrideType) => await _context.ProductOverrides.Where(p => p.OverrideType == overrideType && p.ProductCode == product).FirstOrDefaultAsync();

    /// <summary>
    /// update or add new record (note: EF will add a new record for update if the Id is not set)
    /// </summary>
    /// <param name="productOverride"></param>
    /// <returns></returns>
    public async Task<ProductOverride> Update(ProductOverride productOverride)
    {
        _context.ProductOverrides.Update(productOverride);
        await _context.SaveChangesAsync();
        return productOverride;
    }
}