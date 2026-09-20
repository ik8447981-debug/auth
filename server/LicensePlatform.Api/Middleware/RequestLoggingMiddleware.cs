using System.Diagnostics;

namespace LicensePlatform.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;
        var requestId = Guid.NewGuid().ToString("N");

        context.Items["RequestId"] = requestId;

        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString.ToString();
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";

        _logger.LogInformation(
            "[{RequestId}] Incoming {Method} {Path}{QueryString} from {RemoteIp} UserAgent={UserAgent} CorrelationId={CorrelationId}",
            requestId, method, path, queryString, remoteIp, userAgent, correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            var logLevel = statusCode >= 500 ? LogLevel.Error
                         : statusCode >= 400 ? LogLevel.Warning
                         : LogLevel.Information;

            _logger.Log(logLevel,
                "[{RequestId}] Completed {Method} {Path}{QueryString} => {StatusCode} in {ElapsedMs}ms CorrelationId={CorrelationId}",
                requestId, method, path, queryString, statusCode, elapsedMs, correlationId);
        }
    }
}
