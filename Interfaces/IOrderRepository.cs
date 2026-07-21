using ShoppingApp.Api.DTOs;

namespace ShoppingApp.Api.Interfaces;

public interface IOrderRepository
{
    Task<int> CreateOrderAsync(
        int userId,
        CreateOrderRequest request);
}

