using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using ShoppingApp.Api.DTOs;
using ShoppingApp.Api.Interfaces;

namespace ShoppingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private const string ProductsCacheTag =
        "products";

    private readonly IProductService
        _productService;

    private readonly IOutputCacheStore
        _outputCacheStore;

    public ProductsController(
        IProductService productService,
        IOutputCacheStore outputCacheStore)
    {
        _productService =
            productService;

        _outputCacheStore =
            outputCacheStore;
    }

    [AllowAnonymous]
    [HttpGet]
    [OutputCache(
        PolicyName = "ProductsCache")]
    public async Task<IActionResult> GetAll(
        [FromQuery]
        ProductQueryParameters queryParameters,
        CancellationToken cancellationToken)
    {
        var result =
            await _productService.GetAllAsync(
                queryParameters,
                cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [OutputCache(
        Duration = 30,
        Tags = new[]
        {
            ProductsCacheTag
        })]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var product =
            await _productService.GetByIdAsync(
                id,
                cancellationToken);

        if (product is null)
        {
            return NotFound(new
            {
                message =
                    "Product was not found."
            });
        }

        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm]
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var productId =
            await _productService.CreateAsync(
                request);

        await ClearProductCacheAsync(
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = productId
            },
            new
            {
                id = productId,
                message =
                    "Product created successfully."
            });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        int id,
        [FromForm]
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var wasUpdated =
            await _productService.UpdateAsync(
                id,
                request);

        if (!wasUpdated)
        {
            return NotFound(new
            {
                message =
                    "Product was not found."
            });
        }

        await ClearProductCacheAsync(
            cancellationToken);

        return Ok(new
        {
            message =
                "Product updated successfully."
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var wasDeleted =
            await _productService.DeleteAsync(
                id);

        if (!wasDeleted)
        {
            return NotFound(new
            {
                message =
                    "Product was not found."
            });
        }

        await ClearProductCacheAsync(
            cancellationToken);

        return Ok(new
        {
            message =
                "Product deleted successfully."
        });
    }

    private async Task ClearProductCacheAsync(
        CancellationToken cancellationToken)
    {
        await _outputCacheStore
            .EvictByTagAsync(
                ProductsCacheTag,
                cancellationToken);
    }
}