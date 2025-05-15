using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data.Common;

namespace MWW_Api.Http.Middleware.Health;
public class SQLDbHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    public string ConnectionString { get; }

    public SQLDbHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            try
            {
                await connection.OpenAsync(cancellationToken);
                var command = connection.CreateCommand();
                command.CommandText = "select 1";
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (DbException ex)
            {
                return new HealthCheckResult(status: HealthStatus.Unhealthy, exception: ex);
            }
        }
        return HealthCheckResult.Healthy();
    }
}