using Microsoft.Data.SqlClient;
using shopping_app_dotnet_api.DTOs;
using shopping_app_dotnet_api.Interfaces;

namespace shopping_app_dotnet_api.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly DbConnectionFactory _dbConnectionFactory;

    public OrderRepository(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string orderSql = @"
INSERT INTO Orders
(
    CustomerName,
    Email,
    Phone,
    Address,
    City,
    State,
    Pincode,
    GrandTotal
)
VALUES
(
    @CustomerName,
    @Email,
    @Phone,
    @Address,
    @City,
    @State,
    @Pincode,
    @GrandTotal
);

SELECT CAST(SCOPE_IDENTITY() AS INT);
";

            using var command = new SqlCommand(
                orderSql,
                connection,
                transaction
            );

            command.Parameters.AddWithValue("@CustomerName", request.CustomerName);
            command.Parameters.AddWithValue("@Email", request.Email);
            command.Parameters.AddWithValue("@Phone", request.Phone);
            command.Parameters.AddWithValue("@Address", request.Address);
            command.Parameters.AddWithValue("@City", request.City);
            command.Parameters.AddWithValue("@State", request.State);
            command.Parameters.AddWithValue("@Pincode", request.Pincode);
            command.Parameters.AddWithValue("@GrandTotal", request.GrandTotal);

            int orderId = (int)await command.ExecuteScalarAsync();

            const string orderItemSql = @"
INSERT INTO OrderItems
(
    OrderId,
    ProductId,
    Quantity,
    Price
)
VALUES
(
    @OrderId,
    @ProductId,
    @Quantity,
    @Price
);
";

            foreach (var item in request.Items)
            {
                using var itemCommand = new SqlCommand(
                    orderItemSql,
                    connection,
                    transaction
                );

                itemCommand.Parameters.AddWithValue("@OrderId", orderId);
                itemCommand.Parameters.AddWithValue("@ProductId", item.ProductId);
                itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                itemCommand.Parameters.AddWithValue("@Price", item.Price);

                await itemCommand.ExecuteNonQueryAsync();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
