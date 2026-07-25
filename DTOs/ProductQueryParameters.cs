namespace ShoppingApp.Api.DTOs;

public class ProductQueryParameters
{
    public string? Search { get; set; }

    public string? Category { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }
}