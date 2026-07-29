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

public async Task<PagedResult<Product>> GetAllAsync(
    ProductQueryParameters queryParameters,
    CancellationToken cancellationToken)
{
    var products = new List<Product>();

    using var connection =
        _dbConnectionFactory.CreateConnection();

    await connection.OpenAsync(
        cancellationToken);

    var conditions = new List<string>();

    if (!string.IsNullOrWhiteSpace(
        queryParameters.Search))
    {
        conditions.Add("""
            (
                Name LIKE @Search
                OR Description LIKE @Search
            )
            """);
    }

    if (!string.IsNullOrWhiteSpace(
        queryParameters.Category))
    {
        conditions.Add(
            "Category = @Category");
    }

    if (queryParameters.MinPrice.HasValue)
    {
        conditions.Add(
            "Price >= @MinPrice");
    }

    if (queryParameters.MaxPrice.HasValue)
    {
        conditions.Add(
            "Price <= @MaxPrice");
    }

    var whereClause =
        conditions.Count > 0
            ? " WHERE " +
              string.Join(
                  " AND ",
                  conditions)
            : "";

    void AddFilterParameters(
        SqlCommand command)
    {
        if (!string.IsNullOrWhiteSpace(
            queryParameters.Search))
        {
            command.Parameters.AddWithValue(
                "@Search",
                $"%{queryParameters.Search.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(
            queryParameters.Category))
        {
            command.Parameters.AddWithValue(
                "@Category",
                queryParameters.Category.Trim());
        }

        if (queryParameters.MinPrice.HasValue)
        {
            command.Parameters.AddWithValue(
                "@MinPrice",
                queryParameters.MinPrice.Value);
        }

        if (queryParameters.MaxPrice.HasValue)
        {
            command.Parameters.AddWithValue(
                "@MaxPrice",
                queryParameters.MaxPrice.Value);
        }
    }

    var countQuery =
        "SELECT COUNT(*) FROM Products" +
        whereClause;

    using var countCommand =
        new SqlCommand(
            countQuery,
            connection);

    AddFilterParameters(
        countCommand);

    var countResult =
        await countCommand.ExecuteScalarAsync(
            cancellationToken);

    var totalItems =
        Convert.ToInt32(countResult);

    var sortColumn =
        queryParameters.SortBy?.ToLowerInvariant()
            switch
            {
                "price" => "Price",
                "name" => "Name",
                _ => "Id"
            };

    var sortDirection =
        queryParameters.SortDirection
            ?.ToLowerInvariant() == "desc"
                ? "DESC"
                : "ASC";

    var offset =
        (queryParameters.PageNumber - 1) *
        queryParameters.PageSize;

    var query = $"""
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
        {whereClause}
        ORDER BY {sortColumn} {sortDirection}
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;
        """;

    using var command =
        new SqlCommand(
            query,
            connection);

    AddFilterParameters(command);

    command.Parameters.AddWithValue(
        "@Offset",
        offset);

    command.Parameters.AddWithValue(
        "@PageSize",
        queryParameters.PageSize);

    using var reader =
        await command.ExecuteReaderAsync(
            cancellationToken);

    while (
        await reader.ReadAsync(
            cancellationToken))
    {
        products.Add(new Product
        {
            Id =
                Convert.ToInt32(
                    reader["Id"]),

            Name =
                reader["Name"].ToString()!,

            Description =
                reader["Description"] ==
                DBNull.Value
                    ? null
                    : reader["Description"]
                        .ToString(),

            Category =
                reader["Category"] ==
                DBNull.Value
                    ? null
                    : reader["Category"]
                        .ToString(),

            Price =
                Convert.ToDecimal(
                    reader["Price"]),

            ImageUrl =
                reader["ImageUrl"] ==
                DBNull.Value
                    ? null
                    : reader["ImageUrl"]
                        .ToString(),

            Stock =
                Convert.ToInt32(
                    reader["Stock"]),

            CreatedDate =
                Convert.ToDateTime(
                    reader["CreatedDate"])
        });
    }

    return new PagedResult<Product>
    {
        Items = products,

        PageNumber =
            queryParameters.PageNumber,

        PageSize =
            queryParameters.PageSize,

        TotalItems =
            totalItems,

        TotalPages =
            (int)Math.Ceiling(
                totalItems /
                (double)queryParameters.PageSize)
    };
}
   public async Task<Product?> GetByIdAsync(
    int id,
    CancellationToken cancellationToken = default)
{
    using var connection =
        _dbConnectionFactory.CreateConnection();

    await connection.OpenAsync(
        cancellationToken);

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
        WHERE Id = @Id;
        """;

    using var command =
        new SqlCommand(
            query,
            connection);

    command.Parameters.AddWithValue(
        "@Id",
        id);

    using var reader =
        await command.ExecuteReaderAsync(
            cancellationToken);

    if (
        !await reader.ReadAsync(
            cancellationToken))
    {
        return null;
    }

    return new Product
    {
        Id =
            Convert.ToInt32(
                reader["Id"]),

        Name =
            reader["Name"].ToString()!,

        Description =
            reader["Description"] ==
            DBNull.Value
                ? null
                : reader["Description"]
                    .ToString(),

        Category =
            reader["Category"] ==
            DBNull.Value
                ? null
                : reader["Category"]
                    .ToString(),

        Price =
            Convert.ToDecimal(
                reader["Price"]),

        ImageUrl =
            reader["ImageUrl"] ==
            DBNull.Value
                ? null
                : reader["ImageUrl"]
                    .ToString(),

        Stock =
            Convert.ToInt32(
                reader["Stock"]),

        CreatedDate =
            Convert.ToDateTime(
                reader["CreatedDate"])
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

