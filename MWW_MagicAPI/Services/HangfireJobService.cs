using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MWW_Api.Services;
using MWW_MagicAPI.Data.Contexts;
using System.Collections;

public class HangfireJobService : IRecurringJobService
{
    private const string serverName = "MWWMagicApi";

    private readonly IRecurringJobManager _recurringJobs;
    private readonly PeepsDbContext _context;

    public HangfireJobService(IRecurringJobManager recurringJobs, PeepsDbContext context)
    {
        _recurringJobs = recurringJobs;
        _context = context;
    }

    [Queue("datasync")]
    public async Task Job()
    {
        await ScheduleJobsFromDatabase();
        await InactiveJobsFromDatabase();
    }

    private async Task ScheduleJobsFromDatabase()
    {
        var activeJobs = await _context.HangfireJobs.Where(j => j.IsActive && j.Server == serverName).ToListAsync();

        foreach (var job in activeJobs)
        {
            // 1. Resolve the Type from the string stored in DB
            var serviceType = GetTypeFromName(job.ServiceTypeName.Trim());
            if (serviceType == null) continue;

            // 2. Find the method (usually "Job" or "Execute")
            var method = serviceType.GetMethod(job.JobName.Trim()); // Or store method name in DB too
            if (method == null) continue;

            // 3. Create the Hangfire Job object
            var args = string.IsNullOrEmpty(job.Parameters)
                       ? Array.Empty<object>()
                       : new object[] { job.Parameters };

            var hangfireJob = new Hangfire.Common.Job(serviceType, method, args);

            // 4. Register using the non-generic signature
            _recurringJobs.AddOrUpdate(
                recurringJobId: job.JobId,
                hangfireJob,
                cronExpression: job.CronExpression,
                new RecurringJobOptions { QueueName = job.Queue }
            );
        }
    }

    private async Task InactiveJobsFromDatabase()
    {
        var inactiveJobs = await _context.HangfireJobs.Where(j => !j.IsActive).ToListAsync();

        foreach (var job in inactiveJobs)
        {
            _recurringJobs.RemoveIfExists(job.JobId);
        }
    }

    public static Type? GetTypeFromName(string typeName)
    {
        // Try the default way first
        var type = Type.GetType(typeName);
        if (type != null) return type;

        // Look through all assemblies loaded in the current AppDomain
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type != null) return type;
        }

        return null;
    }
}