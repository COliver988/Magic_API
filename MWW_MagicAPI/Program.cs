using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using MWW_Api.Config;
using MWW_Api.Http.Middleware.Health;
using MWW_Api.Repositories.Exenta;
using MWW_Api.Repositories.Magic;
using MWW_MagicAPI.Data.Models;
using MWW_MagicAPI.Services;
using Prometheus;
using Serilog;
using System.Text;


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
    builder.Services.UseHttpClientMetrics();

    // Exenta
    builder.Services.AddScoped<ICustomerBOLShipmentRepository, CustomerBOLShipmentRepository>();
    builder.Services.AddScoped<IInvoiceOrderHeaderRepository, InvoiceOrderHeaderRepository>();
    builder.Services.AddScoped<IOrderHeaderRepository, OrderHeaderRepository>();
    builder.Services.AddScoped<IGetBatchUnitValues, GetBatchUnitValues>();

    // Magic
    builder.Services.AddScoped<IDapPartnersRepository, DapPartnersRepository>();
    builder.Services.AddScoped<IStuckProductionOrderRepository, StuckProductionOrderRepository>();
    builder.Services.AddScoped<IProductOverrideRepository, ProductOverrideRepository>();
    builder.Services.AddScoped<IMWW_ApplicationRepository, MWW_ApplicationRepository>();
    builder.Services.AddScoped<IUndefinedProductsRepository, UndefinedProductsRepository>();

    // db context
    builder.Services.AddDbContext<MagicDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database:Magic")));
    builder.Services.AddDbContext<ExentaDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database:Exenta")));
    builder.Services.AddDbContext<SerilogDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Database:Serilog")));
    
    // Shopfloor
    builder.Services.AddDbContext<ShopfloorHVDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database:ShopfloorHV")));
    builder.Services.AddDbContext<ShopfloorPDDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database:ShopfloorPD")));
    builder.Services.AddDbContext<ShopfloorTJDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database:ShopfloorTJ")));
    builder.Services.AddDbContext<ShopfloorGMDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database:ShopfloorGM")));

    // factories and services
    builder.Services.AddScoped<IShopfloorDbContextFactory, ShopfloorDbContextFactory>();
    builder.Services.AddScoped<IFixBatchService, FixBatchService>();



    // Add configuration from appsettings.json or other sources
    builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("AuthSettings"));

    // logging settings
    builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
       loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

    // Register your service
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IOrderReportService, OrderReportService>();

    builder.Services.AddHealthChecks()
       .AddCheck("Magic DB",
           new SQLDbHealthCheck(builder.Configuration.GetConnectionString("Database:Magic")),
           HealthStatus.Unhealthy,
           new string[] { "Magic DB", "Database" })
       .AddCheck("Shopfloor Hendersonville DB",
           new SQLDbHealthCheck(builder.Configuration.GetConnectionString("Database:ShopfloorHV")),
           HealthStatus.Degraded,
           new string[] { "Shopfloor Hendersonville DB", "Database" })
       .AddCheck("Shopfloor Spindale DB",
           new SQLDbHealthCheck(builder.Configuration.GetConnectionString("Database:ShopfloorPD")),
           HealthStatus.Degraded,
           new string[] { "Shopfloor Spindale DB", "Database" })
       .AddCheck("Shopfloor Tijuana DB",
           new SQLDbHealthCheck(builder.Configuration.GetConnectionString("Database:ShopfloorTJ")),
           HealthStatus.Degraded,
           new string[] { "Shopfloor Tijuana DB", "Database" })
       .AddCheck("Shopfloor Germany DB",
           new SQLDbHealthCheck(builder.Configuration.GetConnectionString("Database:ShopfloorGM")),
           HealthStatus.Degraded,
           new string[] { "Shopfloor Germany DB", "Database" })
       .AddCheck("Exenta DB",
           new SQLDbHealthCheck(builder.Configuration.GetConnectionString("Database:Exenta")),
           HealthStatus.Unhealthy,
           new string[] { "Exenta DB", "Database" })
       .AddCheck("Hvl Shopfloor File Access",
           new ShopfloorAccessCheck(builder.Configuration.GetValue<string>("Shopfloor:mww")),
           HealthStatus.Degraded,
           new string[] { "Shopfloor Access", "File System" });

    AuthSettings authSettings = builder.Configuration.GetSection("AuthSettings").Get<AuthSettings>();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireAudience = false,
            ValidAudience = authSettings.Audience,
            ValidIssuer = authSettings.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.PrivateKey))
        };
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage();
    }

    //app.UseHsts();
    app.UseHttpMetrics();
    app.UseMetricServer();
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.UseHealthChecks("/api/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    { ResponseWriter = HealthCheckExtensions.WriteResponse });

    Log.Information(builder.Configuration.GetConnectionString("Database:Serilog"));
    Log.Information(builder.Configuration.GetConnectionString("Database:Magic"));
    Log.Information(builder.Configuration.GetConnectionString("Database:Exenta"));
    Log.Information(builder.Configuration.GetConnectionString("Database:ShopfloorPD"));

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
