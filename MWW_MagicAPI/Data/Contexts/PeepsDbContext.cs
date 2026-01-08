using Microsoft.EntityFrameworkCore;
using MWW_Api.Models.Peeps.Printify;

namespace MWW_MagicAPI.Data.Contexts;

public class PeepsDbContext : DbContext
{
    public PeepsDbContext(DbContextOptions<PeepsDbContext> options) : base(options)
    {
    }

    public DbSet<PrintifyOrder> PrintifyOrders { get; set; }
    public DbSet<PrintifyEvent> PrintifyEvents { get; set; }
    public DbSet<PrintifyItem> PrintifyItems { get; set; }
}
