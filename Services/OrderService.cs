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
        await _orderRepository.CreateOrderAsync(request);
    }
}
