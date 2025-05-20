using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Magic;

namespace MWW_Api.Repositories.Magic;
public class MWW_ApplicationRepository : IMWW_ApplicationRepository
{
    private readonly MagicDbContext _context;

    public MWW_ApplicationRepository(MagicDbContext context)
    {
        _context = context;
    }
    public async Task<List<MWW_Applications>> GetActive() => await  _context.MWW_Applications.Where(a => a.Active == true).ToListAsync();

    public async Task<MWW_Applications?> GetByName(string name) => await _context.MWW_Applications
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.RunningApps.ToLower() == name.ToLower());  
}