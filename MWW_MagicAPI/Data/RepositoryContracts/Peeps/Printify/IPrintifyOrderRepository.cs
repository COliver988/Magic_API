using MWW_Api.Models.Peeps.Printify;

namespace MWW_MagicAPI.Data.RepositoryContracts.Peeps.Printify;

public interface IPrintifyOrderRepository
{
    Task<PrintifyOrder?> GetByOrderPOAsync(string po);
    Task<bool> UpdateAsync(string po, string newStatus);
}
