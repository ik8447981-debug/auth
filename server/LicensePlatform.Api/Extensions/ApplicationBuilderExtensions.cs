using LicensePlatform.Api.Middleware;
using Serilog;

namespace LicensePlatform.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder ConfigureMiddlewarePipeline(this IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        // ── Global Exception Handler (first in pipeline) ──────────────────────
        app.UseMiddleware<GlobalExceptionHandler>();

        // ── HTTPS Redirection ────────────────────────────────────────────────
        if (!env.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // ── Correlation ID ────────────────────────────────────────────────────
        app.UseMiddleware<CorrelationIdMiddleware>();

        // ── Request Logging ───────────────────────────────────────────────────
        app.UseMiddleware<RequestLoggingMiddleware>();

        // ── Rate Limiting ─────────────────────────────────────────────────────
        app.UseMiddleware<RateLimitingMiddleware>();

        // ── CORS ──────────────────────────────────────────────────────────────
        app.UseCors("AllowConfiguredOrigins");

        // ── Swagger (available in all environments for API docs) ──────────────
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "api/swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "License Platform API v1");
            c.RoutePrefix = "api/swagger";
        });

        // ── Static Files ──────────────────────────────────────────────────────
        app.UseStaticFiles();

        // ── Routing ───────────────────────────────────────────────────────────
        app.UseRouting();

        // ── Authentication & Authorization ────────────────────────────────────
        app.UseAuthentication();
        app.UseAuthorization();

        // ── Endpoints ───────────────────────────────────────────────────────
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/health", () => Results.Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                environment = env.EnvironmentName
            })).AllowAnonymous();

            endpoints.MapControllers();
        });

        return app;
    }

    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var start = System.Diagnostics.Stopwatch.StartNew();
            await next();
            start.Stop();
            context.Response.Headers["X-Response-Time"] = $"{start.ElapsedMilliseconds}ms";
        });

        return app;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
            {
                context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
            }

            await next();
        });

        return app;
    }
}
