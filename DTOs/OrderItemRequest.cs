namespace ShoppingApp.Api.DTOs;

public class OrderItemRequest
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}

