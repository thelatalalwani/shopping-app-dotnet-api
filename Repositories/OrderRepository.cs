using Microsoft.Data.SqlClient;
using ShoppingApp.Api.DTOs;
using ShoppingApp.Api.Interfaces;

namespace ShoppingApp.Api.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly DbConnectionFactory _dbConnectionFactory;

    public OrderRepository(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<int> CreateOrderAsync(
        int userId,
        CreateOrderRequest request)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string orderSql = @"
INSERT INTO Orders
(
    UserId,
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
    @UserId,
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

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@CustomerName", request.CustomerName);
            command.Parameters.AddWithValue("@Email", request.Email);
            command.Parameters.AddWithValue("@Phone", request.Phone);
            command.Parameters.AddWithValue("@Address", request.Address);
            command.Parameters.AddWithValue("@City", request.City);
            command.Parameters.AddWithValue("@State", request.State);
            command.Parameters.AddWithValue("@Pincode", request.Pincode);
            command.Parameters.AddWithValue("@GrandTotal", request.GrandTotal);

            var result = await command.ExecuteScalarAsync();

            if (result is null)
            {
                throw new InvalidOperationException(
                    "Order could not be created.");
            }

            int orderId = Convert.ToInt32(result);

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

            return orderId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}

