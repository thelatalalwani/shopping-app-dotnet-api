using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShoppingApp.Api.Configuration;
using ShoppingApp.Api.Interfaces;
using ShoppingApp.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShoppingApp.Api.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(
        IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(
        User user,
        out DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.Key))
        {
            throw new InvalidOperationException(
                "JWT key is not configured.");
        }

        expiresAt = DateTime.UtcNow.AddMinutes(
            _jwtSettings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Name,
                user.Name),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Role,
                user.Role),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
