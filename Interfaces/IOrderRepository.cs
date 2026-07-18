using shopping_app_dotnet_api.DTOs;

namespace shopping_app_dotnet_api.Interfaces;

public interface IOrderRepository
{
    Task CreateOrderAsync(CreateOrderRequest request);
}
