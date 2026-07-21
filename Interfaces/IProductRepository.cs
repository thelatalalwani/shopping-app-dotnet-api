using ShoppingApp.Api.Interfaces.Models;

namespace ShoppingApp.Api.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
}

