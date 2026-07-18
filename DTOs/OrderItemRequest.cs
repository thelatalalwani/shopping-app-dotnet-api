namespace shopping_app_dotnet_api.DTOs;

public class OrderItemRequest
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}
