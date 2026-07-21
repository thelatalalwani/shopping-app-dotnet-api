using ShoppingApp.Api.DTOs;

namespace ShoppingApp.Api.Interfaces;

public interface IOrderRepository
{
    Task CreateOrderAsync(CreateOrderRequest request);
}

