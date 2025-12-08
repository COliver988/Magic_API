namespace MWW_Api.Repositories.Magic;

public interface IMilestoneMappingRepository
{
    Task<List<MilestoneMapper>> GetAllMilestoneMappingsAsync();
}