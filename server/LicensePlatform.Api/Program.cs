using LicensePlatform.Api.Extensions;
using LicensePlatform.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "LicensePlatform.Api")
    .WriteTo.Console()
    .WriteTo.File("logs/license-platform-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "License Platform API",
        Version = "v1",
        Description = "Multi-EXE License Management Platform API"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register all application services via extension methods
builder.Services.AddApplicationServices(builder.Configuration);

// Register CORS
builder.Services.AddCorsPolicy(builder.Configuration);

// ── Memory Cache (used by rate limiting) ─────────────────────────────────────
builder.Services.AddMemoryCache();

var app = builder.Build();

// ── Run Database Migrations ─────────────────────────────────────────────────
try
{
    Log.Information("Running database migrations...");
    app.Services.RunDatabaseMigrations();
    Log.Information("Database migrations completed successfully.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Database migration failed. Application cannot start.");
    throw;
}

// ── Seed Default Admin ──────────────────────────────────────────────────────
try
{
    Log.Information("Seeding default admin user...");
    app.Services.SeedDefaultAdmin(app.Configuration);
    Log.Information("Default admin user seeded successfully.");
}
catch (Exception ex)
{
    Log.Warning(ex, "Failed to seed default admin user. Continuing startup...");
}

// ── Middleware Pipeline ──────────────────────────────────────────────────────
app.ConfigureMiddlewarePipeline();

// ── Start ────────────────────────────────────────────────────────────────────
try
{
    Log.Information("Starting License Platform API on {Urls}", string.Join(", ", app.Urls));
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
