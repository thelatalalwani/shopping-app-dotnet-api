namespace shopping_app_dotnet_api.DTOs;

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Pincode { get; set; } = string.Empty;

    public decimal GrandTotal { get; set; }

    public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
}
