using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Api.DTOs.Auth;
using ShoppingApp.Api.Interfaces;
using System.Security.Claims;

namespace ShoppingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request)
    {
        var response =
            await _authService.RegisterAsync(request);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request)
    {
        var response =
            await _authService.LoginAsync(request);

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult GetCurrentUser()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        var name =
            User.FindFirstValue(
                ClaimTypes.Name);

        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var role =
            User.FindFirstValue(
                ClaimTypes.Role);

        return Ok(new
        {
            id = userId,
            name,
            email,
            role
        });
    }
}
