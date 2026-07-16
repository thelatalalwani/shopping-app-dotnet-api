namespace shopping_app_dotnet_api.Interfaces.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public int Stock { get; set; }

    public DateTime CreatedDate { get; set; }
}