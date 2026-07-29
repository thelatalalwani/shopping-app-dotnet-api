using ShoppingApp.Api.DTOs;
using ShoppingApp.Api.Interfaces.Models;

namespace ShoppingApp.Api.Interfaces;

public interface IProductService
{
    Task<PagedResult<Product>> GetAllAsync(
        ProductQueryParameters queryParameters,
        CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<int> CreateAsync(
        CreateProductRequest request);

    Task<bool> UpdateAsync(
        int id,
        UpdateProductRequest request);

    Task<bool> DeleteAsync(int id);
}