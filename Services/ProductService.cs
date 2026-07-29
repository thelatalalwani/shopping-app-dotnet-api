using ShoppingApp.Api.DTOs;
using ShoppingApp.Api.Interfaces;
using ShoppingApp.Api.Interfaces.Models;

namespace ShoppingApp.Api.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IImageService _imageService;

   public ProductService(
    IProductRepository productRepository,
    IImageService imageService)
    {
        _productRepository = productRepository;
        _imageService = imageService;
    }

  public async Task<PagedResult<Product>> GetAllAsync(
    ProductQueryParameters queryParameters,
    CancellationToken cancellationToken)
{
    if (
        queryParameters.MinPrice.HasValue &&
        queryParameters.MinPrice.Value < 0)
    {
        throw new ArgumentException(
            "Minimum price cannot be negative.");
    }

    if (
        queryParameters.MaxPrice.HasValue &&
        queryParameters.MaxPrice.Value < 0)
    {
        throw new ArgumentException(
            "Maximum price cannot be negative.");
    }

    if (
        queryParameters.MinPrice.HasValue &&
        queryParameters.MaxPrice.HasValue &&
        queryParameters.MinPrice.Value >
        queryParameters.MaxPrice.Value)
    {
        throw new ArgumentException(
            "Minimum price cannot exceed maximum price.");
    }

    if (queryParameters.PageNumber <= 0)
    {
        queryParameters.PageNumber = 1;
    }

    if (queryParameters.PageSize <= 0)
    {
        queryParameters.PageSize = 5;
    }

    if (queryParameters.PageSize > 50)
    {
        queryParameters.PageSize = 50;
    }

    return await _productRepository.GetAllAsync(
        queryParameters,
        cancellationToken);
}

  public async Task<Product?> GetByIdAsync(
    int id,
    CancellationToken cancellationToken)
{
    if (id <= 0)
    {
        throw new ArgumentException(
            "Product ID must be greater than zero.");
    }

    return await _productRepository.GetByIdAsync(
        id,
        cancellationToken);
}

 public async Task<int> CreateAsync(
    CreateProductRequest request)
    {
        ValidateProduct(
            request.Name,
            request.Price,
            request.Stock);

        if (request.ImageFile is not null)
        {
            request.ImageUrl =
                await _imageService.SaveProductImageAsync(
                    request.ImageFile);
        }

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

        var existingProduct =
            await _productRepository.GetByIdAsync(id);

        if (existingProduct is null)
        {
            return false;
        }

        var oldImagePath = existingProduct.ImageUrl;
        string? newImagePath = null;

        if (request.ImageFile is not null)
        {
            newImagePath =
                await _imageService.SaveProductImageAsync(
                    request.ImageFile);

            request.ImageUrl = newImagePath;
        }
        else if (string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            request.ImageUrl = oldImagePath;
        }

        var wasUpdated =
            await _productRepository.UpdateAsync(
                id,
                request);

        if (wasUpdated &&
            newImagePath is not null &&
            !string.Equals(
                oldImagePath,
                newImagePath,
                StringComparison.OrdinalIgnoreCase))
        {
            _imageService.DeleteProductImage(
                oldImagePath);
        }

        if (!wasUpdated && newImagePath is not null)
        {
            _imageService.DeleteProductImage(
                newImagePath);
        }

        return wasUpdated;
    }
    
  public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "Product ID must be greater than zero.");
        }

        var existingProduct =
            await _productRepository.GetByIdAsync(id);

        if (existingProduct is null)
        {
            return false;
        }

        var wasDeleted =
            await _productRepository.DeleteAsync(id);

        if (wasDeleted)
        {
            _imageService.DeleteProductImage(
                existingProduct.ImageUrl);
        }

        return wasDeleted;
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