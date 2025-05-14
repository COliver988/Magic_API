using Microsoft.EntityFrameworkCore;
using MWW_Api.Models.Exenta;

namespace MWW_Api.Config;

public class ExentaDbContext : DbContext
{
    public ExentaDbContext(DbContextOptions<ExentaDbContext> options) : base(options) { }

    public DbSet<CustomerBOLShipment> CustomerBOLShipments { get; set; }
    public DbSet<InvoiceOrderHeader> InvoiceOrderHeaders { get; set; }
    public DbSet<OrderHeader> OrderHeaders { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Specify the schema for the entity
        modelBuilder.Entity<CustomerBOLShipment>().ToTable("CustomerBOLShipment", schema: "dbo");
        modelBuilder.Entity<InvoiceOrderHeader>().ToTable("InvoiceOrderHeader", schema: "dbo");
        modelBuilder.Entity<OrderHeader>().ToTable("OrderHeader", schema: "dbo");
    }
}