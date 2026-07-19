using System;
using System.Linq;
using shopping_app_dotnet_api.DTOs;
using shopping_app_dotnet_api.Interfaces;

namespace shopping_app_dotnet_api.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ArgumentException("Order must contain at least one item.");
        }

        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            throw new ArgumentException("Customer name is required.");
        }

        if (request.Items.Any(item => item.Quantity <= 0))
        {
            throw new ArgumentException("Product quantity must be greater than zero.");
        }

        await _orderRepository.CreateOrderAsync(request);
    }
}
