using Microsoft.Data.SqlClient;
using ShoppingApp.Api.Interfaces;
using ShoppingApp.Api.Models;
using System.Data;

namespace ShoppingApp.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection was not found.");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string query = """
            SELECT
                Id,
                Name,
                Email,
                PasswordHash,
                Role,
                CreatedAt
            FROM Users
            WHERE Email = @Email;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(query, connection);

        command.Parameters.Add(
            "@Email",
            SqlDbType.NVarChar,
            255).Value = email;

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapUser(reader);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        const string query = """
            SELECT
                Id,
                Name,
                Email,
                PasswordHash,
                Role,
                CreatedAt
            FROM Users
            WHERE Id = @Id;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(query, connection);

        command.Parameters.Add(
            "@Id",
            SqlDbType.Int).Value = userId;

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapUser(reader);
    }

    public async Task<int> CreateAsync(User user)
    {
        const string query = """
            INSERT INTO Users
            (
                Name,
                Email,
                PasswordHash,
                Role
            )
            VALUES
            (
                @Name,
                @Email,
                @PasswordHash,
                @Role
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(query, connection);

        command.Parameters.Add(
            "@Name",
            SqlDbType.NVarChar,
            100).Value = user.Name;

        command.Parameters.Add(
            "@Email",
            SqlDbType.NVarChar,
            255).Value = user.Email;

        command.Parameters.Add(
            "@PasswordHash",
            SqlDbType.NVarChar,
            -1).Value = user.PasswordHash;

        command.Parameters.Add(
            "@Role",
            SqlDbType.NVarChar,
            20).Value = user.Role;

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        if (result is null)
        {
            throw new InvalidOperationException(
                "User could not be created.");
        }

        return Convert.ToInt32(result);
    }

    private static User MapUser(SqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(
                reader.GetOrdinal("Id")),

            Name = reader.GetString(
                reader.GetOrdinal("Name")),

            Email = reader.GetString(
                reader.GetOrdinal("Email")),

            PasswordHash = reader.GetString(
                reader.GetOrdinal("PasswordHash")),

            Role = reader.GetString(
                reader.GetOrdinal("Role")),

            CreatedAt = reader.GetDateTime(
                reader.GetOrdinal("CreatedAt"))
        };
    }
}
