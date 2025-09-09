using MWW_Api.Models.Shopfloor;

namespace MWW_MagicAPI.Services;
public interface IFixBatchService
{
     Task<List<Unit>> GetMissingBatches(string batchId);   
}