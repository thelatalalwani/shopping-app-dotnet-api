using shopping_app_dotnet_api.Interfaces.Models;

namespace shopping_app_dotnet_api.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();

    Task<Product?> GetByIdAsync(int id);
}
