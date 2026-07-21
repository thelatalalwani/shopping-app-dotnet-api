using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Api.DTOs;
using ShoppingApp.Api.Interfaces;
using System.Security.Claims;

namespace ShoppingApp.Api.Controllers;

[Authorize]
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
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(
            userIdValue,
            out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "The token does not contain a valid user ID."
            });
        }

        var orderId =
            await _orderService.CreateOrderAsync(
                userId,
                request);

        return Ok(new
        {
            orderId,
            message =
                "Order placed successfully."
        });
    }
}

