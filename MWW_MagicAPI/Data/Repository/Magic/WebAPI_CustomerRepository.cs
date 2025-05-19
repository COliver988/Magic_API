using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;
using MWW_Api.Repositories.Magic;

namespace MWW_MagicAPI.Data.Repository.Magic;

public class WebAPI_CustomerRepository : IWebAPI_CustomersRepository
{
    private readonly MagicDbContext _context;

    public WebAPI_CustomerRepository(MagicDbContext context)
    {
        _context = context;
    }
    public async Task<WebAPI_Customers?> GetAccount(string email) => await _context.WebAPI_Customers
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Email == email);
}