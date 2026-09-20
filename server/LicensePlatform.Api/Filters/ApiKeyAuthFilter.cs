using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LicensePlatform.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ApiKeyAuthFilter : Attribute, IAsyncAuthorizationFilter
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyAuthFilter> _logger;

    public ApiKeyAuthFilter(IConfiguration configuration, ILogger<ApiKeyAuthFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Skip if [AllowAnonymous] is present
        if (context.ActionDescriptor.EndpointMetadata.Any(e => e is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute))
        {
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            _logger.LogWarning("API key was not provided in request to {Path}", context.HttpContext.Request.Path);
            context.Result = new UnauthorizedObjectResult(new
            {
                error = new
                {
                    message = "API key is required. Provide it via the X-Api-Key header.",
                    statusCode = 401
                }
            });
            return;
        }

        var configuredApiKey = _configuration["ApiKey"];

        if (string.IsNullOrEmpty(configuredApiKey))
        {
            _logger.LogError("API key is not configured in application settings.");
            context.Result = new StatusCodeResult(500);
            return;
        }

        if (!configuredApiKey.Equals(extractedApiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Invalid API key provided for request to {Path}", context.HttpContext.Request.Path);
            context.Result = new UnauthorizedObjectResult(new
            {
                error = new
                {
                    message = "Invalid API key.",
                    statusCode = 401
                }
            });
            return;
        }

        await Task.CompletedTask;
    }
}
