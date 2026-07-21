using System;
using System.Linq;
using ShoppingApp.Api.DTOs;
using ShoppingApp.Api.Interfaces;

namespace ShoppingApp.Api.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<int> CreateOrderAsync(
        int userId,
        CreateOrderRequest request)
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

        return await _orderRepository.CreateOrderAsync(
            userId,
            request);
    }
}

