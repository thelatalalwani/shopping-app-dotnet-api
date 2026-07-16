using Microsoft.Data.SqlClient;
using shopping_app_dotnet_api.Interfaces;
using shopping_app_dotnet_api.Interfaces.Models;

namespace shopping_app_dotnet_api.Interfaces.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DbConnectionFactory _dbConnectionFactory;

    public ProductRepository(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        var products = new List<Product>();

        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.OpenAsync();

        string query = @"
            SELECT
                Id,
                Name,
                Description,
                Price,
                ImageUrl,
                Stock,
                CreatedDate
            FROM Products";

        using var command = new SqlCommand(query, connection);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString()!,
                Description = reader["Description"]?.ToString(),
                Price = Convert.ToDecimal(reader["Price"]),
                ImageUrl = reader["ImageUrl"]?.ToString(),
                Stock = Convert.ToInt32(reader["Stock"]),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
            });
        }

        return products;
    }
}