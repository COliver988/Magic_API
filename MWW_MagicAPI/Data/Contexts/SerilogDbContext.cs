using Microsoft.EntityFrameworkCore;
using MWW_MagicAPI.Models.Logging;

namespace MWW_Api.Config;

public class SerilogDbContext : DbContext
{

    public SerilogDbContext(DbContextOptions<SerilogDbContext> options) : base(options)
    {
    }
    public DbSet<SeriLog> SeriLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SeriLog>(serilog =>
        {
            serilog.OwnsOne(x => x.LogEvent, le =>
            {
                le.ToJson();
                le.OwnsOne(x => x.Properties);
            });
        });
        base.OnModelCreating(modelBuilder);
    }
}