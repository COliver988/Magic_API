using Microsoft.EntityFrameworkCore;
using MWW_Api.Models.Exenta;

namespace MWW_Api.Config;

public class ExentaDbContext : DbContext
{
    public ExentaDbContext(DbContextOptions<ExentaDbContext> options) : base(options) { }

    public DbSet<CustomerBOLShipment> CustomerBOLShipments { get; set; }
    public DbSet<InvoiceOrderHeader> InvoiceOrderHeaders { get; set; }
    public DbSet<OrderHeader> OrderHeaders { get; set; }
    public DbSet<ProdOrderHeader> ProdOrderHeaders { get; set; }
    public DbSet<ProdOrderDetail> ProdOrderDetails { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Specify the schema for the entity
        modelBuilder.Entity<CustomerBOLShipment>().ToTable("CustomerBOLShipment", schema: "dbo");
        modelBuilder.Entity<InvoiceOrderHeader>().ToTable("InvoiceOrderHeader", schema: "dbo");
        modelBuilder.Entity<OrderHeader>().ToTable("OrderHeader", schema: "dbo");
        modelBuilder.Entity<ProdOrderHeader>().ToTable("ProdOrderHeader", schema: "dbo");
        modelBuilder.Entity<ProdOrderDetail>().ToTable("ProdOrderDetail", schema: "dbo");
    }
}