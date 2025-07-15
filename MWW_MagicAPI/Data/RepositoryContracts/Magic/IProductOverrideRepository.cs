using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;
public interface IProductOverrideRepository
{
    Task<List<ProductOverride>> GetAllByOverrideType(int? overrideType);
    Task<List<ProductOverride>> GetByProduct(string product);
    Task<ProductOverride?> GetByProductAndOverrideType(string product, int overrideType);
    Task<ProductOverride>? GetByIdAsync(int id);
    Task Delete(ProductOverride productOverride);
    Task<ProductOverride> Update(ProductOverride productOverride);
}