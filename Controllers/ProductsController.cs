using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Api.Interfaces;

namespace ShoppingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllAsync();

        return Ok(products);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }
}

