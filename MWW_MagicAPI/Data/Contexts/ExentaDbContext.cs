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
    public DbSet<PickOrderHeader> PickOrderHeaders { get; set; }
    public DbSet<PickOrderDetail> PickOrderDetails { get; set; }
    public DbSet<Style> Styles { get; set; }
    public DbSet<StyleItem> StyleItems { get; set; }
    public DbSet<Size> Sizes { get; set; }
    public DbSet<Dimension> Dimensions { get; set; }
    public DbSet<Color> Colors { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Specify the schema for the entity
        modelBuilder.Entity<CustomerBOLShipment>().ToTable("CustomerBOLShipment", schema: "dbo").HasNoKey();
        modelBuilder.Entity<InvoiceOrderHeader>().ToTable("InvoiceOrderHeader", schema: "dbo").HasNoKey();
        modelBuilder.Entity<OrderHeader>().ToTable("OrderHeader", schema: "dbo").HasNoKey();
        modelBuilder.Entity<ProdOrderHeader>().ToTable("ProdOrderHeader", schema: "dbo").HasNoKey();
        modelBuilder.Entity<ProdOrderDetail>().ToTable("ProdOrderDetail", schema: "dbo").HasNoKey();
        modelBuilder.Entity<PickOrderHeader>().ToTable("PickOrderHeader", schema: "dbo").HasNoKey();
        modelBuilder.Entity<PickOrderDetail>().ToTable("PickOrderDetail", schema: "dbo").HasNoKey();
        modelBuilder.Entity<Style>().ToTable("Style", schema: "dbo").HasNoKey();
        modelBuilder.Entity<StyleItem>().ToTable("StyleItem", schema: "dbo").HasNoKey();
        modelBuilder.Entity<Size>().ToTable("Size", schema: "dbo").HasNoKey();
        modelBuilder.Entity<Dimension>().ToTable("Dimension", schema: "dbo").HasNoKey();
        modelBuilder.Entity<Color>().ToTable("Color", schema: "dbo").HasNoKey();
    }
}