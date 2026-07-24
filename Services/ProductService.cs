using ShoppingApp.Api.DTOs;
using ShoppingApp.Api.Interfaces;
using ShoppingApp.Api.Interfaces.Models;

namespace ShoppingApp.Api.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(
        IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "Product ID must be greater than zero.");
        }

        return await _productRepository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(
        CreateProductRequest request)
    {
        ValidateProduct(
            request.Name,
            request.Price,
            request.Stock);

        return await _productRepository.CreateAsync(request);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateProductRequest request)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "Product ID must be greater than zero.");
        }

        ValidateProduct(
            request.Name,
            request.Price,
            request.Stock);

        return await _productRepository.UpdateAsync(
            id,
            request);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "Product ID must be greater than zero.");
        }

        return await _productRepository.DeleteAsync(id);
    }

    private static void ValidateProduct(
        string name,
        decimal price,
        int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Product name is required.");
        }

        if (price <= 0)
        {
            throw new ArgumentException(
                "Product price must be greater than zero.");
        }

        if (stock < 0)
        {
            throw new ArgumentException(
                "Product stock cannot be negative.");
        }
    }
}