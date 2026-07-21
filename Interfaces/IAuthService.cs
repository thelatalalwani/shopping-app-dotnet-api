using ShoppingApp.Api.DTOs.Auth;

namespace ShoppingApp.Api.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request);

    Task<AuthResponse> LoginAsync(
        LoginRequest request);
}
