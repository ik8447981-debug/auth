using System.Net;
using System.Text.Json;

namespace LicensePlatform.Api.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred. Request: {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ArgumentException argEx => (HttpStatusCode.BadRequest, argEx.Message),
            KeyNotFoundException keyEx => (HttpStatusCode.NotFound, keyEx.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access."),
            InvalidOperationException opEx => (HttpStatusCode.Conflict, opEx.Message),
            TimeoutException => (HttpStatusCode.GatewayTimeout, "The request timed out."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
        };

        context.Response.StatusCode = (int)statusCode;

        var requestId = context.Items["RequestId"]?.ToString() ?? context.TraceIdentifier;

        var response = new
        {
            error = new
            {
                message,
                statusCode = (int)statusCode,
                requestId,
                timestamp = DateTime.UtcNow
            }
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
