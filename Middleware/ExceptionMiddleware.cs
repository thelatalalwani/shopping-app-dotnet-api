using System.Net;
using System.Text.Json;

namespace ShoppingApp.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(
                exception,
                "Validation error occurred.");

            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.BadRequest,
                exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogWarning(
                exception,
                "Authentication failed.");

            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.Unauthorized,
                exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An unexpected error occurred.");

            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType =
            "application/json";

        var response = new
        {
            message
        };

        var json =
            JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}


