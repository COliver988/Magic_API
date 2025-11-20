namespace MWW_MagicAPI.Services;

public interface IUpdateExentaStatusesService
{
    // Updates Exenta statuses for records that have a timestamp less than the specified minutes ago
    void UpdateExentaStatuses(int minutes);
}
