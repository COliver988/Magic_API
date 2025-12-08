using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;
public interface IMilestoneMapperRepository
{
    Task<List<MilestoneMapper>> GetAllMilestoneMappingsAsync();
}