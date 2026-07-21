using ShoppingApp.Api.Interfaces.Models;

namespace ShoppingApp.Api.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();

    Task<Product?> GetByIdAsync(int id);
}

