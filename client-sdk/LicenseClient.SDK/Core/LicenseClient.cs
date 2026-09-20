using System.Net;
using LicenseClient.SDK.Caching;
using LicenseClient.SDK.Exceptions;
using LicenseClient.SDK.Features;
using LicenseClient.SDK.Identity;
using LicenseClient.SDK.Models;
using LicenseClient.SDK.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LicenseClient.SDK.Core;

/// <summary>
/// Main entry point for license management. EXE developers use this class
/// to activate, validate, and manage licenses in their applications.
/// </summary>
public class LicenseClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly LicenseConfiguration _config;
    private readonly ILicenseCache _cache;
    private readonly ISignatureVerifier _signatureVerifier;
    private readonly IDeviceIdentity _deviceIdentity;
    private readonly IFeatureManager _featureManager;
    private readonly ServerTimeValidator _serverTimeValidator;
    private readonly ILogger<LicenseClient> _logger;
    private readonly LicenseManager _licenseManager;
    private bool _disposed;

    /// <summary>
    /// Event raised when the license status changes.
    /// </summary>
    public event EventHandler<LicenseStatus>? StatusChanged
    {
        add => _licenseManager.StatusChanged += value;
        remove => _licenseManager.StatusChanged -= value;
    }

    /// <summary>
    /// Creates a new LicenseClient with the specified configuration.
    /// </summary>
    /// <param name="config">License configuration. Must be fully populated.</param>
    /// <param name="httpClient">Optional custom HttpClient. If null, a new one is created.</param>
    /// <param name="logger">Optional logger. If null, a null logger is used.</param>
    public LicenseClient(
        LicenseConfiguration config,
        HttpClient? httpClient = null,
        ILogger<LicenseClient>? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? NullLogger<LicenseClient>.Instance;

        // Validate configuration
        _config.Validate();

        // Create or use provided HttpClient
        _httpClient = httpClient ?? CreateDefaultHttpClient();

        // Initialize components
        _cache = new LicenseCache(
            _config.ProductId,
            _config.CacheDirectory);

        _deviceIdentity = new DeviceIdentity(_config.ProductId);

        var featureManager = new FeatureManager();
        _featureManager = featureManager;

        _signatureVerifier = new SignatureVerifier();
        _serverTimeValidator = new ServerTimeValidator();

        var validator = new LicenseValidator(
            _signatureVerifier,
            _serverTimeValidator,
            _config.PublicKeyPem);

        // Create the license manager
        _licenseManager = new LicenseManager(
            _config,
            _httpClient,
            _cache,
            _deviceIdentity,
            _featureManager,
            validator,
            _signatureVerifier,
            _serverTimeValidator,
            _logger as ILogger<LicenseManager> ?? NullLogger<LicenseManager>.Instance);

        _logger.LogInformation(
            "LicenseClient created for product {ProductId} (v{Version})",
            _config.ProductId, _config.ApplicationVersion);
    }

    /// <summary>
    /// Creates a LicenseClient with full dependency injection support.
    /// </summary>
    /// <param name="config">License configuration.</param>
    /// <param name="httpClient">HttpClient instance.</param>
    /// <param name="cache">License cache implementation.</param>
    /// <param name="deviceIdentity">Device identity implementation.</param>
    /// <param name="featureManager">Feature manager implementation.</param>
    /// <param name="signatureVerifier">Signature verifier implementation.</param>
    /// <param name="serverTimeValidator">Server time validator.</param>
    /// <param name="licenseValidator">License validator implementation.</param>
    /// <param name="licenseManager">License manager instance.</param>
    /// <param name="logger">Logger instance.</param>
    internal LicenseClient(
        LicenseConfiguration config,
        HttpClient httpClient,
        ILicenseCache cache,
        IDeviceIdentity deviceIdentity,
        IFeatureManager featureManager,
        ISignatureVerifier signatureVerifier,
        ServerTimeValidator serverTimeValidator,
        ILicenseValidator licenseValidator,
        LicenseManager licenseManager,
        ILogger<LicenseClient> logger)
    {
        _config = config;
        _httpClient = httpClient;
        _cache = cache;
        _deviceIdentity = deviceIdentity;
        _featureManager = featureManager;
        _signatureVerifier = signatureVerifier;
        _serverTimeValidator = serverTimeValidator;
        _logger = logger;
        _licenseManager = licenseManager;
    }

    /// <summary>
    /// Initializes the license client. Loads cached state, validates it,
    /// and starts background validation if a valid license exists.
    /// Must be called before any other operations.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await _licenseManager.InitializeAsync(cancellationToken);
    }

    /// <summary>
    /// Activates a license key with the server.
    /// </summary>
    /// <param name="licenseKey">The license key to activate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>License information if activation succeeds.</returns>
    /// <exception cref="LicenseActivationException">Activation failed.</exception>
    /// <exception cref="ProductMismatchException">License is for a different product.</exception>
    /// <exception cref="SignatureVerificationException">Server response signature invalid.</exception>
    /// <exception cref="ClockRollbackException">Clock manipulation detected.</exception>
    public async Task<LicenseInfo> ActivateAsync(string licenseKey, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return await _licenseManager.ActivateAsync(licenseKey, cancellationToken);
    }

    /// <summary>
    /// Validates the current license. Performs local validation first,
    /// then contacts the server for revalidation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="LicenseValidationException">Validation failed.</exception>
    /// <exception cref="OfflineGraceExpiredException">Offline grace period exhausted.</exception>
    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await _licenseManager.ValidateAsync(cancellationToken);
    }

    /// <summary>
    /// Deactivates the license, removing local state and notifying the server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DeactivateAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await _licenseManager.DeactivateAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the current license status without making any network calls.
    /// </summary>
    public LicenseStatus GetStatus()
    {
        ThrowIfDisposed();
        return _licenseManager.GetStatus();
    }

    /// <summary>
    /// Gets comprehensive license information from the cached state.
    /// </summary>
    public LicenseInfo GetLicenseInfo()
    {
        ThrowIfDisposed();
        return _licenseManager.GetLicenseInfo();
    }

    /// <summary>
    /// Checks if a specific feature is enabled in the current license.
    /// Features are sourced from the server-signed license response.
    /// </summary>
    /// <param name="featureKey">The feature key to check.</param>
    /// <returns>True if the feature is enabled; false otherwise.</returns>
    public bool HasFeature(string featureKey)
    {
        ThrowIfDisposed();
        return _licenseManager.HasFeature(featureKey);
    }

    /// <summary>
    /// Gets the license expiry date, if available.
    /// </summary>
    public DateTime? GetExpiry()
    {
        ThrowIfDisposed();
        return _licenseManager.GetExpiry();
    }

    /// <summary>
    /// Gets the current plan/tier name.
    /// </summary>
    public string GetPlan()
    {
        ThrowIfDisposed();
        return _licenseManager.GetPlan();
    }

    /// <summary>
    /// Gets all enabled feature keys.
    /// </summary>
    public IReadOnlyList<string> GetAllFeatures()
    {
        ThrowIfDisposed();
        return _featureManager.GetAllFeatures();
    }

    /// <summary>
    /// Gets the device fingerprint for this machine.
    /// </summary>
    public string GetDeviceFingerprint()
    {
        ThrowIfDisposed();
        return _deviceIdentity.GetFingerprint();
    }

    /// <summary>
    /// Checks if a cached license exists on disk.
    /// </summary>
    public bool HasCachedLicense()
    {
        ThrowIfDisposed();
        return _cache.IsValid();
    }

    /// <summary>
    /// Clears the local license cache without deactivating with the server.
    /// Use with caution - this forces re-activation.
    /// </summary>
    public void ClearCache()
    {
        ThrowIfDisposed();
        _cache.Clear();
        _featureManager.ClearFeatures();
        _logger.LogInformation("License cache cleared");
    }

    /// <summary>
    /// Checks if the license is currently valid (without network calls).
    /// Useful for quick checks in performance-sensitive paths.
    /// </summary>
    public bool IsLicenseValid()
    {
        ThrowIfDisposed();
        return _licenseManager.GetStatus() == LicenseStatus.Active;
    }

    private static HttpClient CreateDefaultHttpClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseDefaultCredentials = false
        };

        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _licenseManager.Dispose();
        _httpClient.Dispose();

        GC.SuppressFinalize(this);
    }
}
