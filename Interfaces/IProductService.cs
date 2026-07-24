using ShoppingApp.Api.DTOs;
using ShoppingApp.Api.Interfaces.Models;

namespace ShoppingApp.Api.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();

    Task<Product?> GetByIdAsync(int id);

    Task<int> CreateAsync(CreateProductRequest request);

    Task<bool> UpdateAsync(
        int id,
        UpdateProductRequest request);

    Task<bool> DeleteAsync(int id);
}