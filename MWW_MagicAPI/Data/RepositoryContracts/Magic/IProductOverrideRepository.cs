using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;
public interface IProductOverrideRepository
{
    Task<List<ProductOverride>> GetAllByOverrideType(int overrideType);
    Task<ProductOverride?> GetByProductAndOverrideType(string product, int overrideType);
    Task<List<ProductOverride>> GetAll();
    Task<ProductOverride>? GetByIdAsync(long id);
    Task Delete(ProductOverride productOverride);
    Task<ProductOverride> Update(ProductOverride productOverride);
}