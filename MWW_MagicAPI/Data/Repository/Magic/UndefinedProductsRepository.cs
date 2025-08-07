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

    public Task<List<UndefinedProduct>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpsertUndefinedProduct(string customerId, string productCode, string vendorPo, int interfaceId)
    {
        try
        {
            var result = await _context.Database.ExecuteSqlRawAsync
                ($"[dbo].[UpsertUndefinedProduct] @customer_id ='{customerId}', @vendor_po ='{vendorPo}', @product_code ='{productCode}', @interface_id = '{interfaceId}'");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
        return false;
    }
}
