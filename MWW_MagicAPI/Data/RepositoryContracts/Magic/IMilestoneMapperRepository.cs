using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;
public interface IMilestoneMapperRepository
{
    Task<List<MilestoneMapper>> GetAllMilestoneMappingsAsync();
    Task<MilestoneMapper?> GetMilestoneMappingByIdAsync(int id);
    Task<MilestoneMapper> AddMilestoneMappingAsync(MilestoneMapper milestoneMapper); 
    Task<MilestoneMapper> UpdateMilestoneMappingAsync(MilestoneMapper milestoneMapper);
}