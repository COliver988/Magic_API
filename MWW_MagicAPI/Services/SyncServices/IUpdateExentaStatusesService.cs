namespace MWW_MagicAPI.Services.SyncServices;
public interface IUpdateExentaStatusesService
{
    Task<int> UpdateExentaStatuses(int minutes);
}