using Microsoft.EntityFrameworkCore;
using MWW_Api.Models.Magic;
using MWW_MagicAPI.Data.Models.Magic;

namespace MWW_Api.Config;

public class MagicDbContext : DbContext
{
    public MagicDbContext(DbContextOptions<MagicDbContext> options) : base(options) { }
    public DbSet<DapPartner> DapPartners { get; set; }
    public DbSet<ProductOverride> ProductOverrides { get; set; }
    public DbSet<StuckProductionOrders> StuckProductionOrders { get; set; }
    public DbSet<MWW_Applications> MWW_Applications { get; set; }
    public DbSet<WebAPI_Customer> WebAPI_Customers { get; set; }
    public DbSet<UndefinedProduct> UndefinedProducts { get; set; }
    public DbSet<DyePrintDetails> DyePrintDetails { get; set; }
    public DbSet<ExentaPOLinesWithAckNo> ExentaPOLinesWithAckNos { get; set; }
    public DbSet<DyeItemAttributes> DyeItemAttributes { get; set; }
    public DbSet<DyePrintHeader> DyePrintHeaders { get; set; }
    public DbSet<UPCLogIn> UPCLogIns { get; set; }
    public DbSet<WebItem> WebItems { get; set; }
    public DbSet<MilestoneMapper> MilestoneMappers { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Specify the schema for the entity
        modelBuilder.Entity<DapPartner>().ToTable("DAP_PARTNERS", schema: "dbo").HasNoKey();
        modelBuilder.Entity<ProductOverride>().ToTable("product_overrides", schema: "dbo");
        modelBuilder.Entity<UndefinedProduct>().ToTable("undefined_products", schema: "dbo");
        modelBuilder.Entity<MWW_Applications>().ToTable("MWW_APPLICATIONS", schema: "dbo").HasNoKey();
        modelBuilder.Entity<StuckProductionOrders>().ToTable("stuck_production_orders", schema: "dbo").HasNoKey();
        modelBuilder.Entity<WebAPI_Customer>().ToTable("WebAPI_Customers", schema: "dbo").HasNoKey();
        modelBuilder.Entity<DyePrintDetails>().ToTable("dyePrintDetails", schema: "dbo").HasNoKey();
        modelBuilder.Entity<ExentaPOLinesWithAckNo>().ToTable("Exenta_PO_Lines_with_AckNo", schema: "dbo").HasNoKey();
        modelBuilder.Entity<DyeItemAttributes>().ToTable("dyeitem_Attributes", schema: "dbo").HasNoKey();
        modelBuilder.Entity<DyePrintHeader>().ToTable("dyePrintHeader", schema: "dbo").HasNoKey();
        modelBuilder.Entity<UPCLogIn>().ToTable("UPC_LOG_IN", schema: "dbo");
        modelBuilder.Entity<MilestoneMapper>().ToTable("milestone_mapper", schema: "dbo");
        modelBuilder.Entity<WebItem>().ToTable("WEB_Items", schema: "dbo").HasNoKey();
    }
}