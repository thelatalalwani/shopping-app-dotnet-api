using Microsoft.AspNetCore.Http;

namespace ShoppingApp.Api.Interfaces;

public interface IImageService
{
    Task<string> SaveProductImageAsync(IFormFile imageFile);

    void DeleteProductImage(string? imagePath);
}