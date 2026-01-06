using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public interface ISFCTimestampRepository
{
    public Task<List<SFCTimestamp>> GetAllAsync();
    public Task<SFCTimestamp?> GetByLocationAsync(string location);
    public Task<SFCTimestamp> UpdateAsync(SFCTimestamp sFCTimestamp);
}
