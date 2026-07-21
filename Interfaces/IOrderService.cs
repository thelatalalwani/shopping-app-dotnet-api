using ShoppingApp.Api.DTOs;

namespace ShoppingApp.Api.Interfaces;

public interface IOrderService
{
    Task CreateOrderAsync(CreateOrderRequest request);
}

