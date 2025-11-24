namespace MWW_MagicAPI.Services;
public interface IUpdateExentaStatusesService
{
    Task<bool> UpdateExentaStatuses(int minutes);
}