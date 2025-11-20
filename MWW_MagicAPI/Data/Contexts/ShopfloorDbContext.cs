using Microsoft.EntityFrameworkCore;
using MWW_Api.Models.Shopfloor;

namespace MWW_Api.Config;

public class ShopfloorDbContext : DbContext
{
    public ShopfloorDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Unit> Units { get; set; }
    public DbSet<MileStone> MileStones { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<ProductOperation> ProductOperations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Unit>().ToTable("Unit", schema: "dbo");
    }
}

public class ShopfloorHVDbContext : ShopfloorDbContext
{
    public ShopfloorHVDbContext(DbContextOptions<ShopfloorHVDbContext> options)
        : base(options) { }
}

public class ShopfloorPDDbContext : ShopfloorDbContext
{
    public ShopfloorPDDbContext(DbContextOptions<ShopfloorPDDbContext> options)
        : base(options) { }
}

public class ShopfloorTJDbContext : ShopfloorDbContext
{
    public ShopfloorTJDbContext(DbContextOptions<ShopfloorTJDbContext> options)
        : base(options) { }
}

public class ShopfloorGMDbContext : ShopfloorDbContext
{
    public ShopfloorGMDbContext(DbContextOptions<ShopfloorGMDbContext> options)
        : base(options) { }
}