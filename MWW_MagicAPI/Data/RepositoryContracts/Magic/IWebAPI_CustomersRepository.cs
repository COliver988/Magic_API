using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public interface IWebAPI_CustomersRepositoruy
{
    Task<WebAPI_Customer> GetByEmail(string email);
}
