using ShoppingApp.Api.Models;

namespace ShoppingApp.Api.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(
        User user,
        out DateTime expiresAt);
}
