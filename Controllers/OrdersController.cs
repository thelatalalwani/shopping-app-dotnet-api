using Microsoft.AspNetCore.Mvc;
using shopping_app_dotnet_api.DTOs;
using shopping_app_dotnet_api.Interfaces;

namespace shopping_app_dotnet_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;

    public OrdersController(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        CreateOrderRequest request)
    {
        await _orderRepository.CreateOrderAsync(request);

        return Ok();
    }
}
