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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrintifyItem>()
            .HasOne<PrintifyOrder>() // The Item has one Order
            .WithMany(o => o.PrintifyItems) // The Order has many Items
            .HasForeignKey(i => i.OrderId); // Use "OrderId" instead of "PrintifyOrderId"
    }
}
