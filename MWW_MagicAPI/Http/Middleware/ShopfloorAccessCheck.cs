using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MWW_Api.Http.Middleware.Health;

public class ShopfloorAccessCheck : IHealthCheck
{
    private readonly string _path;
    public ShopfloorAccessCheck(string path)
    {
        _path = path;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            string testFile = System.IO.Path.Combine(_path, "Test", "healthcheck.txt");
            File.WriteAllText(testFile, DateTime.Now.ToString());
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
                return HealthCheckResult.Healthy();
            }
            else
            {
                return new HealthCheckResult(status: HealthStatus.Degraded, description: "Healthcheck file was not created.");
            }
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(status: HealthStatus.Degraded, exception: ex);
        }
    }
}
