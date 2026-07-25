using Microsoft.Data.SqlClient;
using ShoppingApp.Api.DTOs;
using ShoppingApp.Api.Interfaces;
using ShoppingApp.Api.Interfaces.Models;

namespace ShoppingApp.Api.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DbConnectionFactory _dbConnectionFactory;

    public ProductRepository(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }
    
public async Task<List<Product>> GetAllAsync(
    ProductQueryParameters queryParameters)
{
    var products = new List<Product>();

    using var connection =
        _dbConnectionFactory.CreateConnection();

    await connection.OpenAsync();

    var conditions = new List<string>();

    var query = """
        SELECT
            Id,
            Name,
            Description,
            Category,
            Price,
            ImageUrl,
            Stock,
            CreatedDate
        FROM Products
        """;

    using var command = new SqlCommand();
    command.Connection = connection;

    if (!string.IsNullOrWhiteSpace(
        queryParameters.Search))
    {
        conditions.Add("""
            (
                Name LIKE @Search
                OR Description LIKE @Search
            )
            """);

        command.Parameters.AddWithValue(
            "@Search",
            $"%{queryParameters.Search.Trim()}%");
    }

    if (!string.IsNullOrWhiteSpace(
        queryParameters.Category))
    {
        conditions.Add("Category = @Category");

        command.Parameters.AddWithValue(
            "@Category",
            queryParameters.Category.Trim());
    }

    if (queryParameters.MinPrice.HasValue)
    {
        conditions.Add("Price >= @MinPrice");

        command.Parameters.AddWithValue(
            "@MinPrice",
            queryParameters.MinPrice.Value);
    }

    if (queryParameters.MaxPrice.HasValue)
    {
        conditions.Add("Price <= @MaxPrice");

        command.Parameters.AddWithValue(
            "@MaxPrice",
            queryParameters.MaxPrice.Value);
    }

    if (conditions.Count > 0)
    {
        query +=
            " WHERE " +
            string.Join(" AND ", conditions);
    }

    var sortColumn =
        queryParameters.SortBy?.ToLower() switch
        {
            "price" => "Price",
            "name" => "Name",
            _ => "Id"
        };

    var sortDirection =
        queryParameters.SortDirection?.ToLower()
            == "desc"
            ? "DESC"
            : "ASC";

    query +=
        $" ORDER BY {sortColumn} {sortDirection}";

    command.CommandText = query;

    using var reader =
        await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        products.Add(new Product
        {
            Id = Convert.ToInt32(reader["Id"]),
            Name = reader["Name"].ToString()!,
            Description =
                reader["Description"]?.ToString(),
            Category =
                reader["Category"]?.ToString(),
            Price =
                Convert.ToDecimal(reader["Price"]),
            ImageUrl =
                reader["ImageUrl"]?.ToString(),
            Stock =
                Convert.ToInt32(reader["Stock"]),
            CreatedDate =
                Convert.ToDateTime(
                    reader["CreatedDate"])
        });
    }

    return products;
}
    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

       const string query = """
    SELECT
        Id,
        Name,
        Description,
        Category,
        Price,
        ImageUrl,
        Stock,
        CreatedDate
    FROM Products
    WHERE Id = @Id
    """;

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Product
        {
            Id = Convert.ToInt32(reader["Id"]),
            Name = reader["Name"].ToString()!,
            Description = reader["Description"]?.ToString(),
            Category = reader["Category"]?.ToString(),
            Price = Convert.ToDecimal(reader["Price"]),
            ImageUrl = reader["ImageUrl"]?.ToString(),
            Stock = Convert.ToInt32(reader["Stock"]),
            CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
        };
    }


    public async Task<int> CreateAsync(
        CreateProductRequest request)
    {
        using var connection =
            _dbConnectionFactory.CreateConnection();

        await connection.OpenAsync();

        const string query = """
        INSERT INTO Products
        (
            Name,
            Description,
            Category,
            Price,
            ImageUrl,
            Stock,
            CreatedDate
        )
        VALUES
        (
            @Name,
            @Description,
            @Category,
            @Price,
            @ImageUrl,
            @Stock,
            SYSUTCDATETIME()
        );

        SELECT CAST(SCOPE_IDENTITY() AS INT);
        """;

        using var command = new SqlCommand(
            query,
            connection);

        command.Parameters.AddWithValue(
            "@Name",
            request.Name);

        command.Parameters.AddWithValue(
            "@Description",
            (object?)request.Description ?? DBNull.Value);

            command.Parameters.AddWithValue(
    "@Category",
    (object?)request.Category ?? DBNull.Value);

        command.Parameters.AddWithValue(
            "@Price",
            request.Price);

        command.Parameters.AddWithValue(
            "@ImageUrl",
            (object?)request.ImageUrl ?? DBNull.Value);

        command.Parameters.AddWithValue(
            "@Stock",
            request.Stock);

        var result = await command.ExecuteScalarAsync();

        if (result is null)
        {
            throw new InvalidOperationException(
                "Product could not be created.");
        }

        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateProductRequest request)
    {
        using var connection =
            _dbConnectionFactory.CreateConnection();

        await connection.OpenAsync();

        const string query = """
            UPDATE Products
            SET
                Name = @Name,
                Description = @Description,
                Category = @Category,
                Price = @Price,
                ImageUrl = @ImageUrl,
                Stock = @Stock
            WHERE Id = @Id;
            """;

        using var command = new SqlCommand(
            query,
            connection);

        command.Parameters.AddWithValue(
            "@Id",
            id);

        command.Parameters.AddWithValue(
            "@Name",
            request.Name);

        command.Parameters.AddWithValue(
            "@Description",
            (object?)request.Description ?? DBNull.Value);

            command.Parameters.AddWithValue(
            "@Category",
            (object?)request.Category ?? DBNull.Value);

        command.Parameters.AddWithValue(
            "@Price",
            request.Price);

        command.Parameters.AddWithValue(
            "@ImageUrl",
            (object?)request.ImageUrl ?? DBNull.Value);

        command.Parameters.AddWithValue(
            "@Stock",
            request.Stock);

        var affectedRows =
            await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection =
            _dbConnectionFactory.CreateConnection();

        await connection.OpenAsync();

        const string query = """
            DELETE FROM Products
            WHERE Id = @Id;
            """;

        using var command = new SqlCommand(
            query,
            connection);

        command.Parameters.AddWithValue(
            "@Id",
            id);

        var affectedRows =
            await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }
}

