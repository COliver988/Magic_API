using MWW_Api.Models.Peeps.Printify;

namespace MWW_MagicAPI.Data.RepositoryContracts.Peeps.Printify;

public interface IPrintifyEventRepository
{
    Task<List<PrintifyEvent>> GetAllByOrder(long orderId);
    Task<List<PrintifyEvent>> AddEvents(List<PrintifyEvent> printifyEvents);
}
