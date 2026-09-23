using System.Text;
using LicensePlatform.Api.Filters;
using LicensePlatform.Api.Middleware;
using LicensePlatform.Application.Interfaces;
using LicensePlatform.Application.Services;
using LicensePlatform.Infrastructure.Data;
using LicensePlatform.Infrastructure.Repositories;
using LicensePlatform.Security.DeviceFingerprint;
using LicensePlatform.Security.LicenseGeneration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LicensePlatform.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ── Database ──────────────────────────────────────────────────────────
        services.AddDbContext<LicensePlatformDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(LicensePlatformDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                }));

        // ── Authentication (JWT) ──────────────────────────────────────────────
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
        var issuer = jwtSettings["Issuer"] ?? "LicensePlatform";
        var audience = jwtSettings["Audience"] ?? "LicensePlatformApp";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("JWT authentication failed: {Message}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogDebug("JWT token validated for user: {User}",
                        context.Principal?.Identity?.Name ?? "unknown");
                    return Task.CompletedTask;
                }
            };
        });

        // ── Authorization ─────────────────────────────────────────────────────
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin", "SuperAdmin"));

            options.AddPolicy("SuperAdminOnly", policy =>
                policy.RequireRole("SuperAdmin"));
        });

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ILicenseRepository, LicenseRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();

        // ── Application Services ──────────────────────────────────────────────
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<ILicensePlanService, LicensePlanService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService>(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var jwtSecret = configuration["JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");
            var tokenExpirationMinutes = configuration.GetValue("JwtSettings:ExpirationMinutes", 1440);

            return new AuthService(
                serviceProvider.GetRequiredService<LicensePlatformDbContext>(),
                serviceProvider.GetRequiredService<IAuditService>(),
                jwtSecret,
                tokenExpirationMinutes);
        });
        services.AddScoped<ILicenseKeyGenerator, LicenseKeyGenerator>();
        services.AddSingleton<IDeviceFingerprintService, DeviceFingerprintService>();

        // ── Filters ───────────────────────────────────────────────────────────
        services.AddScoped<ApiKeyAuthFilter>();

        // ── HttpClient for external calls if needed ───────────────────────────
        services.AddHttpClient();

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOrigins = configuration.GetSection("CorsOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowConfiguredOrigins", policy =>
            {
                policy.WithOrigins(corsOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }

    public static void RunDatabaseMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LicensePlatformDbContext>();

        // Only apply pending migrations; don't create the database
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
        if (pendingMigrations.Any())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Applying {Count} pending database migrations...", pendingMigrations.Count);
            dbContext.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
    }

    public static void SeedDefaultAdmin(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var superAdminUsername = configuration["SeedData:SuperAdmin:Username"] ?? "admin";
        var superAdminEmail = configuration["SeedData:SuperAdmin:Email"] ?? "admin@licenseplatform.com";
        var superAdminPassword = configuration["SeedData:SuperAdmin:Password"] ?? "Admin@123!";

        try
        {
            var request = new LicensePlatform.Shared.DTOs.Requests.AdminCreateRequest
            {
                Username = superAdminUsername,
                Email = superAdminEmail,
                Password = superAdminPassword,
                Role = "SuperAdmin"
            };
            authService.CreateAdmin(request, Domain.Enums.AdminRole.SuperAdmin);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Failed to seed super admin user.");
        }
    }
}
