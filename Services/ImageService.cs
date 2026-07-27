using Microsoft.AspNetCore.Http;
using ShoppingApp.Api.Interfaces;

namespace ShoppingApp.Api.Services;

public class ImageService : IImageService
{
    private const long MaximumFileSize = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private readonly IWebHostEnvironment _environment;

    public ImageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveProductImageAsync(
        IFormFile imageFile)
    {
        if (imageFile is null || imageFile.Length == 0)
        {
            throw new ArgumentException(
                "Please select an image file.");
        }

        if (imageFile.Length > MaximumFileSize)
        {
            throw new ArgumentException(
                "Image size cannot exceed 5 MB.");
        }

        var extension = Path
            .GetExtension(imageFile.FileName)
            .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                "Only JPG, JPEG, PNG and WEBP images are allowed.");
        }

        var webRootPath = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");
        }

        var uploadFolder = Path.Combine(
            webRootPath,
            "uploads",
            "products");

        Directory.CreateDirectory(uploadFolder);

        var uniqueFileName =
            $"{Guid.NewGuid()}{extension}";

        var physicalFilePath = Path.Combine(
            uploadFolder,
            uniqueFileName);

        await using var stream =
            new FileStream(
                physicalFilePath,
                FileMode.Create);

        await imageFile.CopyToAsync(stream);

        return $"/uploads/products/{uniqueFileName}";
    }

    public void DeleteProductImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return;
        }

        if (!imagePath.StartsWith(
                "/uploads/products/",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var webRootPath = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");
        }

        var relativePath = imagePath
            .TrimStart('/')
            .Replace(
                '/',
                Path.DirectorySeparatorChar);

        var physicalFilePath = Path.Combine(
            webRootPath,
            relativePath);

        if (File.Exists(physicalFilePath))
        {
            File.Delete(physicalFilePath);
        }
    }
}