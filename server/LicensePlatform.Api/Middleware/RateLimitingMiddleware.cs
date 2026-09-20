using System.Collections.Concurrent;
using System.Net;

namespace LicensePlatform.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RateLimitInfo> _clientRequests = new();
    private readonly int _defaultLimit;
    private readonly int _clientEndpointLimit;
    private readonly int _clientEndpointWindowSeconds;
    private readonly int _adminEndpointLimit;
    private readonly int _adminEndpointWindowSeconds;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;

        var rateLimitConfig = configuration.GetSection("RateLimiting");
        _defaultLimit = int.TryParse(rateLimitConfig["DefaultLimit"], out var dl) ? dl : 100;
        _clientEndpointLimit = int.TryParse(rateLimitConfig["ClientEndpointLimit"], out var cel) ? cel : 30;
        _clientEndpointWindowSeconds = int.TryParse(rateLimitConfig["ClientEndpointWindowSeconds"], out var cew) ? cew : 60;
        _adminEndpointLimit = int.TryParse(rateLimitConfig["AdminEndpointLimit"], out var ael) ? ael : 200;
        _adminEndpointWindowSeconds = int.TryParse(rateLimitConfig["AdminEndpointWindowSeconds"], out var aew) ? aew : 60;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.Value ?? "/";

        int limit;
        int windowSeconds;

        if (path.StartsWith("/api/v1/client", StringComparison.OrdinalIgnoreCase))
        {
            limit = _clientEndpointLimit;
            windowSeconds = _clientEndpointWindowSeconds;
        }
        else if (path.StartsWith("/api/v1/admin", StringComparison.OrdinalIgnoreCase))
        {
            limit = _adminEndpointLimit;
            windowSeconds = _adminEndpointWindowSeconds;
        }
        else
        {
            limit = _defaultLimit;
            windowSeconds = 60;
        }

        var key = $"{clientIp}:{(path.StartsWith("/api/v1/client", StringComparison.OrdinalIgnoreCase) ? "client" : "admin")}";
        var now = DateTime.UtcNow;

        var info = _clientRequests.AddOrUpdate(key,
            _ => new RateLimitInfo { WindowStart = now, Count = 1 },
            (_, existing) =>
            {
                if ((now - existing.WindowStart).TotalSeconds > windowSeconds)
                {
                    existing.WindowStart = now;
                    existing.Count = 1;
                }
                else
                {
                    existing.Count++;
                }
                return existing;
            });

        // Clean up stale entries periodically (simple approach)
        if (_clientRequests.Count > 10000)
        {
            var staleKeys = _clientRequests
                .Where(kvp => (now - kvp.Value.WindowStart).TotalSeconds > windowSeconds * 2)
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var staleKey in staleKeys)
            {
                _clientRequests.TryRemove(staleKey, out _);
            }
        }

        context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, limit - info.Count).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(info.WindowStart.AddSeconds(windowSeconds)).ToUnixTimeSeconds().ToString();

        if (info.Count > limit)
        {
            _logger.LogWarning("Rate limit exceeded for IP {ClientIp} on {Path}. Count: {Count}, Limit: {Limit}",
                clientIp, path, info.Count, limit);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            var retryAfter = windowSeconds - (int)(now - info.WindowStart).TotalSeconds;
            context.Response.Headers["Retry-After"] = retryAfter.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    message = "Rate limit exceeded. Please try again later.",
                    statusCode = 429,
                    retryAfter
                }
            });
            return;
        }

        await _next(context);
    }

    private class RateLimitInfo
    {
        public DateTime WindowStart { get; set; }
        public int Count { get; set; }
    }
}
