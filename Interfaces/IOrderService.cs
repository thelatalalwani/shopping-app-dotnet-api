using ShoppingApp.Api.DTOs;

namespace ShoppingApp.Api.Interfaces;

public interface IOrderService
{
    Task<int> CreateOrderAsync(
        int userId,
        CreateOrderRequest request);
}

