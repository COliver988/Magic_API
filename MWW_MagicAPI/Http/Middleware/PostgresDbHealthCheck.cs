using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace MWW_Api.Http.Middleware.Health;
public class PostgresDbHealthCheck : IHealthCheck
{
    private const string DefaultTestQuery = "Select 1";

    private readonly string _connectionString;

    public PostgresDbHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync(cancellationToken);
                    var command = connection.CreateCommand();
                    command.CommandText = "select 1";
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    return new HealthCheckResult(status: HealthStatus.Unhealthy, exception: ex);
                }
                return HealthCheckResult.Healthy();
            }
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(status: HealthStatus.Unhealthy, exception: ex);
        }
    }
}