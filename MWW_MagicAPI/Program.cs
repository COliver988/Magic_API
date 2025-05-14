using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Repositories.Exenta;
using MWW_Api.Repositories.Magic;

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

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
