using MWW_MagicAPI.Data.Models.DTO;

namespace MWW_Api.Repositories.Exenta;

public interface IGetBatchUnitValues
{
    Task<WorkOrderDataDTO?> GetBatchUnitValue(int prodNoCompany, int sequence);
}
