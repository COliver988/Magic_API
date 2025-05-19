using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MWW_Api.Config;
using MWW_Api.Http.Middleware.Health;
using MWW_Api.Repositories.Exenta;
using MWW_Api.Repositories.Magic;
using MWW_MagicAPI.Data.Models;
using MWW_MagicAPI.Services;


var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
        .AddEnvironmentVariables()
        .Build();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Exenta
    builder.Services.AddScoped<ICustomerBOLShipmentRepository, CustomerBOLShipmentRepository>();
    builder.Services.AddScoped<IInvoiceOrderHeaderRepository, InvoiceOrderHeaderRepository>();
    builder.Services.AddScoped<IOrderHeaderRepository, OrderHeaderRepository>();

    // Magic
    builder.Services.AddScoped<IDapPartnersRepository, DapPartnersRepository>();
    builder.Services.AddScoped<IStuckProductionOrderRepository, StuckProductionOrderRepository>();
    builder.Services.AddScoped<IProductOverrideRepository, ProductOverrideRepository>();
    builder.Services.AddScoped<IMWW_ApplicationRepository, MWW_ApplicationRepository>();

    // db context
    builder.Services.AddDbContext<MagicDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database:Magic")));
    builder.Services.AddDbContext<ExentaDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database:Exenta")));

    // Add configuration from appsettings.json or other sources
    builder.Services.Configure<AuthSettings>(configuration.GetSection("AuthSettings"));

    // Register your service
    builder.Services.AddTransient<IAuthService, AuthService>();

    builder.Services.AddHealthChecks()
       .AddCheck("Magic DB",
           new SQLDbHealthCheck(builder.Configuration.GetConnectionString("Database:Magic")),
           HealthStatus.Unhealthy,
           new string[] { "Magic DB", "Database" })
       .AddCheck("Exenta DB",
           new SQLDbHealthCheck(builder.Configuration.GetConnectionString("Database:Exenta")),
           HealthStatus.Unhealthy,
           new string[] { "Exenta DB", "Database" });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.UseHealthChecks("/api/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    { ResponseWriter = HealthCheckExtensions.WriteResponse });
    app.MapGet("/health", () =>
    {
        var yourData = new
        {
            Message = "For API usage, see https://mwwondemand.github.io/#introduction",
            Time = DateTime.UtcNow
        };

        return yourData;
    })
        .WithName("Health Check")
        .WithOpenApi();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
