using Microsoft.AspNetCore.Mvc;
using shopping_app_dotnet_api.Interfaces;

namespace shopping_app_dotnet_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;

    public ProductsController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productRepository.GetAllAsync();

        return Ok(products);
    }
}