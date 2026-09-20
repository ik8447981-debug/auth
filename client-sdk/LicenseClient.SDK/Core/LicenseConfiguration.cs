namespace LicenseClient.SDK.Core;

/// <summary>
/// Configuration for the license client. All settings required
/// for initialization and operation of the SDK.
/// </summary>
public class LicenseConfiguration
{
    /// <summary>
    /// Base URL of the license server (e.g., "https://license.example.com").
    /// </summary>
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Product identifier this application is licensed as (e.g., "CLEANER").
    /// Used for product isolation and multi-EXE separation.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Short product code (e.g., "CLNR"). Used in cache paths and logging.
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>
    /// RSA public key in PEM format for verifying server signatures.
    /// NEVER contains private key material.
    /// </summary>
    public string PublicKeyPem { get; set; } = string.Empty;

    /// <summary>
    /// How often to re-validate with the server (in minutes). Default: 60.
    /// </summary>
    public int ValidationIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// How long the app can work offline after last successful validation (in hours). Default: 24.
    /// </summary>
    public int OfflineGraceHours { get; set; } = 24;

    /// <summary>
    /// Version of the application using this SDK. Reported to server.
    /// </summary>
    public string ApplicationVersion { get; set; } = "1.0.0";

    /// <summary>
    /// HTTP timeout for server requests (in seconds). Default: 30.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of retries for failed server requests. Default: 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Whether to enable detailed debug logging. Default: false.
    /// </summary>
    public bool EnableDebugLogging { get; set; } = false;

    /// <summary>
    /// Custom cache directory override. If null, uses %APPDATA%\{ProductId}.
    /// </summary>
    public string? CacheDirectory { get; set; }

    /// <summary>
    /// Validates the configuration and throws if required fields are missing.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ServerUrl))
            throw new Exceptions.LicenseException("CONFIG_MISSING", "ServerUrl is required.");

        if (!Uri.TryCreate(ServerUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "https" && uri.Scheme != "http"))
            throw new Exceptions.LicenseException("CONFIG_INVALID", "ServerUrl must be a valid HTTP/HTTPS URL.");

        if (string.IsNullOrWhiteSpace(ProductId))
            throw new Exceptions.LicenseException("CONFIG_MISSING", "ProductId is required.");

        if (string.IsNullOrWhiteSpace(ProductCode))
            throw new Exceptions.LicenseException("CONFIG_MISSING", "ProductCode is required.");

        if (string.IsNullOrWhiteSpace(PublicKeyPem))
            throw new Exceptions.LicenseException("CONFIG_MISSING", "PublicKeyPem is required.");

        if (ValidationIntervalMinutes < 5)
            throw new Exceptions.LicenseException("CONFIG_INVALID", "ValidationIntervalMinutes must be at least 5.");

        if (OfflineGraceHours < 1)
            throw new Exceptions.LicenseException("CONFIG_INVALID", "OfflineGraceHours must be at least 1.");

        if (TimeoutSeconds < 5 || TimeoutSeconds > 300)
            throw new Exceptions.LicenseException("CONFIG_INVALID", "TimeoutSeconds must be between 5 and 300.");

        if (MaxRetries < 0 || MaxRetries > 10)
            throw new Exceptions.LicenseException("CONFIG_INVALID", "MaxRetries must be between 0 and 10.");
    }
}
