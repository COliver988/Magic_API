using Microsoft.EntityFrameworkCore;

namespace MWW_Api.Config;

public class HangfireDbContext : DbContext
{
    public HangfireDbContext(DbContextOptions<HangfireDbContext> options) : base(options) { }
}