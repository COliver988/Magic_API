using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public interface IUndefinedProductsRepository
{
    Task<List<UndefinedProduct>> GetAllAsync();
    Task<bool> UpsertUndefinedProduct(string customerId, string productCode, string vendorPo, int interfaceId);

    Task<bool> DeleteUndefinedProducts(List<int> ids);
}
