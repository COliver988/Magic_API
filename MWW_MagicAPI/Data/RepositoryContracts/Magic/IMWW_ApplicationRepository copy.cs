using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;
public interface IWebAPI_CustomersRepository
{
     Task<WebAPI_Customers?> GetAccount(string email);       
}