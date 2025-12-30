using Microsoft.EntityFrameworkCore;

namespace MWW_Api.Config;

public class PostgresDbContext : DbContext
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options)
    {
    }
}