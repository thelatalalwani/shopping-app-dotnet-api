using System;
using System.Linq;
using ShoppingApp.Api.DTOs;
using ShoppingApp.Api.Interfaces;

namespace ShoppingApp.Api.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDiscountCalculator _discountCalculator;

    public OrderService(
        IOrderRepository orderRepository,
        IDiscountCalculator discountCalculator)
    {
        _orderRepository = orderRepository;
        _discountCalculator = discountCalculator;
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

        var amountAfterFestivalDiscount =
            _discountCalculator.ApplyFestivalDiscount(
                request.GrandTotal);

        var finalAmount =
            _discountCalculator.ApplyBankDiscount(
                amountAfterFestivalDiscount);

        request.GrandTotal =
            decimal.Round(
                finalAmount,
                2);

        return await _orderRepository.CreateOrderAsync(
            userId,
            request);
    }
}

