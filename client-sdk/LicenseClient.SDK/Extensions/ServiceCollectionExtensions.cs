using LicenseClient.SDK.Caching;
using LicenseClient.SDK.Core;
using LicenseClient.SDK.Features;
using LicenseClient.SDK.Identity;
using LicenseClient.SDK.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LicenseClient.SDK.Extensions;

/// <summary>
/// Extension methods for registering LicenseClient services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all LicenseClient services with the DI container.
    /// Uses the provided configuration for all license operations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">License configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLicenseClient(
        this IServiceCollection services,
        LicenseConfiguration config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        // Validate configuration at registration time
        config.Validate();

        // Register configuration as singleton
        services.AddSingleton(config);

        // Register core services
        services.TryAddSingleton<ISignatureVerifier, SignatureVerifier>();
        services.TryAddSingleton<ServerTimeValidator>();

        services.TryAddSingleton<IDeviceIdentity>(sp =>
            new DeviceIdentity(config.ProductId));

        services.TryAddSingleton<ILicenseCache>(sp =>
            new LicenseCache(config.ProductId, config.CacheDirectory));

        services.TryAddSingleton<IFeatureManager, FeatureManager>();

        services.TryAddSingleton<ILicenseValidator>(sp =>
        {
            var signatureVerifier = sp.GetRequiredService<ISignatureVerifier>();
            var timeValidator = sp.GetRequiredService<ServerTimeValidator>();
            return new LicenseValidator(signatureVerifier, timeValidator, config.PublicKeyPem);
        });

        // Register HttpClient with timeout from config
        services.TryAddSingleton<HttpClient>(sp =>
        {
            var logger = sp.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
            };
            httpClient.DefaultRequestHeaders.Add("X-Product-Id", config.ProductId);
            httpClient.DefaultRequestHeaders.Add("X-Product-Code", config.ProductCode);
            httpClient.DefaultRequestHeaders.Add("X-App-Version", config.ApplicationVersion);
            httpClient.DefaultRequestHeaders.Add("X-SDK-Version", "1.0.0");
            return httpClient;
        });

        // Register LicenseManager
        services.TryAddSingleton<LicenseManager>(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            var cache = sp.GetRequiredService<ILicenseCache>();
            var deviceIdentity = sp.GetRequiredService<IDeviceIdentity>();
            var featureManager = sp.GetRequiredService<IFeatureManager>();
            var validator = sp.GetRequiredService<ILicenseValidator>();
            var signatureVerifier = sp.GetRequiredService<ISignatureVerifier>();
            var timeValidator = sp.GetRequiredService<ServerTimeValidator>();
            var logger = sp.GetService<ILogger<LicenseManager>>() ?? NullLogger<LicenseManager>.Instance;

            return new LicenseManager(
                config, httpClient, cache, deviceIdentity,
                featureManager, validator, signatureVerifier,
                timeValidator, logger);
        });

        // Register LicenseClient
        services.TryAddSingleton<LicenseClient.SDK.Core.LicenseClient>(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            var cache = sp.GetRequiredService<ILicenseCache>();
            var deviceIdentity = sp.GetRequiredService<IDeviceIdentity>();
            var featureManager = sp.GetRequiredService<IFeatureManager>();
            var signatureVerifier = sp.GetRequiredService<ISignatureVerifier>();
            var timeValidator = sp.GetRequiredService<ServerTimeValidator>();
            var validator = sp.GetRequiredService<ILicenseValidator>();
            var manager = sp.GetRequiredService<LicenseManager>();
            var logger = sp.GetService<ILogger<LicenseClient.SDK.Core.LicenseClient>>() ?? NullLogger<LicenseClient.SDK.Core.LicenseClient>.Instance;

            return new LicenseClient.SDK.Core.LicenseClient(
                config, httpClient, cache, deviceIdentity,
                featureManager, signatureVerifier, timeValidator,
                validator, manager, logger);
        });

        return services;
    }

    /// <summary>
    /// Registers LicenseClient services with a custom HttpClient.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">License configuration.</param>
    /// <param name="configureHttpClient">Action to configure the HttpClient.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLicenseClient(
        this IServiceCollection services,
        LicenseConfiguration config,
        Action<HttpClient> configureHttpClient)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        if (configureHttpClient == null)
            throw new ArgumentNullException(nameof(configureHttpClient));

        // Register the custom HttpClient factory
        services.AddHttpClient("LicenseClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("X-Product-Id", config.ProductId);
            client.DefaultRequestHeaders.Add("X-Product-Code", config.ProductCode);
            client.DefaultRequestHeaders.Add("X-App-Version", config.ApplicationVersion);
            client.DefaultRequestHeaders.Add("X-SDK-Version", "1.0.0");
            configureHttpClient(client);
        });

        return services.AddLicenseClient(config);
    }

    /// <summary>
    /// Registers LicenseClient services with custom HttpClientHandler configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">License configuration.</param>
    /// <param name="configureHandler">Action to configure the HttpClientHandler.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLicenseClient(
        this IServiceCollection services,
        LicenseConfiguration config,
        Action<HttpClientHandler> configureHandler)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        if (configureHandler == null)
            throw new ArgumentNullException(nameof(configureHandler));

        // Replace HttpClient registration with handler-configured version
        services.AddSingleton<HttpClient>(sp =>
        {
            var handler = new HttpClientHandler();
            configureHandler(handler);

            var httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
            };

            httpClient.DefaultRequestHeaders.Add("X-Product-Id", config.ProductId);
            httpClient.DefaultRequestHeaders.Add("X-Product-Code", config.ProductCode);
            httpClient.DefaultRequestHeaders.Add("X-App-Version", config.ApplicationVersion);
            httpClient.DefaultRequestHeaders.Add("X-SDK-Version", "1.0.0");

            return httpClient;
        });

        return services.AddLicenseClient(config);
    }
}
