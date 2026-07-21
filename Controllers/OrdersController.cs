using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Api.DTOs;
using ShoppingApp.Api.Interfaces;

namespace ShoppingApp.Api.Controllers;

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

