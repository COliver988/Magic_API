using MWW_Api.Config;

public class ShopfloorDbContextFactory : IShopfloorDbContextFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ShopfloorDbContextFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ShopfloorDbContext GetContext(string batchId)
    {
        string prefix = batchId.Substring(0, 2).ToUpper();

        return prefix switch
        {
            "HV" => _serviceProvider.GetRequiredService<ShopfloorHVDbContext>(),
            "PD" => _serviceProvider.GetRequiredService<ShopfloorPDDbContext>(),
            "TJ" => _serviceProvider.GetRequiredService<ShopfloorTJDbContext>(),
            "GM" => _serviceProvider.GetRequiredService<ShopfloorGMDbContext>(),
            _ => _serviceProvider.GetRequiredService<ShopfloorHVDbContext>(),
        };
    }
}