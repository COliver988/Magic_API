using MWW_Api.Models.Shopfloor;
using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_MagicAPI.Services;
public interface IFixBatchService
{
     Task<List<Unit>> GetMissingBatches(string batchId);   
}