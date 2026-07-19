using shopping_app_dotnet_api.DTOs;

namespace shopping_app_dotnet_api.Interfaces;

public interface IOrderService
{
    Task CreateOrderAsync(CreateOrderRequest request);
}
