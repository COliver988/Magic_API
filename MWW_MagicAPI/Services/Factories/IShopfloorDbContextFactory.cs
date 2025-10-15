using MWW_Api.Config;

public interface IShopfloorDbContextFactory
{
    ShopfloorDbContext GetContext(string batchId);
}