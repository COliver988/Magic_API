namespace MWW_MagicAPI.Services;
public interface IFixBatchService
{
     Task GetMissingBatches(string batchId);   
}