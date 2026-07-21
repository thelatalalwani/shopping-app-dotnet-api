using ShoppingApp.Api.Models;

namespace ShoppingApp.Api.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(int userId);

    Task<int> CreateAsync(User user);
}
