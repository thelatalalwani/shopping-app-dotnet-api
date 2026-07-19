using Microsoft.AspNetCore.Mvc;
using shopping_app_dotnet_api.DTOs;
using shopping_app_dotnet_api.Interfaces;

namespace shopping_app_dotnet_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        CreateOrderRequest request)
    {
        await _orderService.CreateOrderAsync(request);

        return Ok(new
        {
            message = "Order placed successfully"
        });
    }
}
