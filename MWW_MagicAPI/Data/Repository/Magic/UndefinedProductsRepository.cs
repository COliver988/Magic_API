using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public class UndefinedProductsRepository : IUndefinedProductsRepository
{
    private readonly MagicDbContext _context;
    private readonly ILogger<UndefinedProductsRepository> _logger;

    public UndefinedProductsRepository(MagicDbContext context, ILogger<UndefinedProductsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> DeleteUndefinedProducts(List<int> ids)
    {
        try
        {
            List<UndefinedProduct> products = new();
            foreach (int id in ids)
            {
                products.Add(await _context.UndefinedProducts.FindAsync(id) ?? new UndefinedProduct());
            }
            _context.UndefinedProducts.RemoveRange(products);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return false;
        }
    }

    public async Task<List<UndefinedProduct>> GetAllAsync() => await _context.UndefinedProducts.ToListAsync();

    public async Task<bool> UpsertUndefinedProduct(string customerId, string productCode, string vendorPo, int interfaceId)
    {
        try
        {
            var result = await _context.Database.ExecuteSqlInterpolatedAsync(
               $@"EXEC [dbo].[UpsertUndefinedProduct] 
                  @customer_id = {customerId}, 
                  @vendor_po = {vendorPo}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
        return false;
    }
}
