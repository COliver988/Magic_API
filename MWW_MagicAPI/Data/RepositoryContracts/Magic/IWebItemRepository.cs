using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public interface IWebItemRepository
{
    Task<WebItem?> GetByItemCodeAsync(string itemCode);
}
