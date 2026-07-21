using Microsoft.AspNetCore.Identity;
using ShoppingApp.Api.DTOs.Auth;
using ShoppingApp.Api.Interfaces;
using ShoppingApp.Api.Models;

namespace ShoppingApp.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        var normalizedName = request.Name.Trim();
        var normalizedEmail =
            request.Email.Trim().ToLowerInvariant();

        if (normalizedName.Length < 2)
        {
            throw new ArgumentException(
                "Name must contain at least 2 characters.");
        }

        if (request.Password.Length < 8)
        {
            throw new ArgumentException(
                "Password must contain at least 8 characters.");
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException(
                "Password and confirmation password do not match.");
        }

        var existingUser =
            await _userRepository.GetByEmailAsync(
                normalizedEmail);

        if (existingUser is not null)
        {
            throw new ArgumentException(
                "An account with this email already exists.");
        }

        var user = new User
        {
            Name = normalizedName,
            Email = normalizedEmail,

            // Public registration must never create Admin.
            Role = "Customer"
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                request.Password);

        user.Id =
            await _userRepository.CreateAsync(user);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request)
    {
        var normalizedEmail =
            request.Email.Trim().ToLowerInvariant();

        var user =
            await _userRepository.GetByEmailAsync(
                normalizedEmail);

        // Use one generic message so callers cannot easily
        // discover which email addresses are registered.
        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (verificationResult ==
            PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var token =
            _jwtTokenService.GenerateToken(
                user,
                out var expiresAt);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,

            User = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            }
        };
    }
}
