using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;

public class WebAPI_CustomerRepository : IWebAPI_CustomersRepositoruy
{
    private readonly MagicDbContext _context;

    public WebAPI_CustomerRepository(MagicDbContext context)
    {
        _context = context;
    }
    public async Task<WebAPI_Customer?> GetByEmail(string email) => await _context.WebAPI_Customers
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Email == email);
}