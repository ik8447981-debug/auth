using System.Net.Http.Json;
using System.Text.Json;
using LicenseClient.SDK.Caching;
using LicenseClient.SDK.Exceptions;
using LicenseClient.SDK.Features;
using LicenseClient.SDK.Identity;
using LicenseClient.SDK.Models;
using LicenseClient.SDK.Validation;
using Microsoft.Extensions.Logging;

namespace LicenseClient.SDK.Core;

/// <summary>
/// High-level license management orchestrator.
/// Coordinates activation, validation, caching, feature checks,
/// background validation timer, and offline/online transitions.
/// </summary>
public class LicenseManager : IDisposable
{
    private readonly LicenseConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILicenseCache _cache;
    private readonly IDeviceIdentity _deviceIdentity;
    private readonly IFeatureManager _featureManager;
    private readonly ILicenseValidator _validator;
    private readonly ISignatureVerifier _signatureVerifier;
    private readonly ServerTimeValidator _timeValidator;
    private readonly ILogger<LicenseManager> _logger;

    private Timer? _validationTimer;
    private CachedLicenseState? _currentState;
    private LicenseStatus _currentStatus = LicenseStatus.Unknown;
    private readonly object _lock = new();
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Event raised when license status changes.
    /// </summary>
    public event EventHandler<LicenseStatus>? StatusChanged;

    /// <summary>
    /// Creates a new LicenseManager.
    /// </summary>
    public LicenseManager(
        LicenseConfiguration config,
        HttpClient httpClient,
        ILicenseCache cache,
        IDeviceIdentity deviceIdentity,
        IFeatureManager featureManager,
        ILicenseValidator validator,
        ISignatureVerifier signatureVerifier,
        ServerTimeValidator timeValidator,
        ILogger<LicenseManager> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _deviceIdentity = deviceIdentity ?? throw new ArgumentNullException(nameof(deviceIdentity));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
        _timeValidator = timeValidator ?? throw new ArgumentNullException(nameof(timeValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes the license manager: loads cached state, verifies it,
    /// and starts background validation timer if a valid license exists.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Initializing license manager for product {ProductId}", _config.ProductId);

        // Load cached state
        _currentState = _cache.Load();

        if (_currentState == null)
        {
            _logger.LogDebug("No cached license found");
            SetStatus(LicenseStatus.Unknown);
            return;
        }

        // Restore time validator state
        if (_currentState.LastServerTime != default && _currentState.LastLocalTime != default)
        {
            _timeValidator.RestoreState(_currentState.LastServerTime, _currentState.LastLocalTime);
        }

        // Load features from cached state
        if (_currentState.SignedState?.Features != null)
        {
            _featureManager.LoadFeatures(_currentState.SignedState.Features);
        }

        // Validate cached state
        var outcome = _validator.Validate(_currentState, _config.ProductId, _config.OfflineGraceHours);

        if (outcome.IsValid)
        {
            SetStatus(outcome.Status);
            StartValidationTimer();
            _logger.LogInformation("License initialized: {Status}", _currentStatus);
        }
        else
        {
            _logger.LogWarning("Cached license invalid: {Reason}", outcome.FailureReason);
            SetStatus(outcome.Status);

            // Try to revalidate with server if we have network
            if (_currentState.LicenseKey != null)
            {
                try
                {
                    await ValidateWithServerAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Server revalidation failed during initialization");
                }
            }
        }
    }

    /// <summary>
    /// Activates a license key with the server.
    /// </summary>
    /// <param name="licenseKey">The license key to activate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The license info if activation succeeds.</returns>
    public async Task<LicenseInfo> ActivateAsync(string licenseKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            throw new LicenseActivationException("License key cannot be empty.");

        _logger.LogInformation("Activating license key for product {ProductId}", _config.ProductId);
        SetStatus(LicenseStatus.Activating);

        var fingerprint = _deviceIdentity.GetFingerprint();
        var osVersion = Environment.OSVersion.ToString();

        var request = new ActivationRequest
        {
            LicenseKey = licenseKey,
            ProductId = _config.ProductId,
            DeviceFingerprint = fingerprint,
            ApplicationVersion = _config.ApplicationVersion,
            OSVersion = osVersion
        };

        try
        {
            var response = await SendRequestAsync<SignedLicenseResponse>(
                "api/license/activate", request, cancellationToken);

            if (response == null)
                throw new LicenseActivationException("Empty response from license server.");

            // Verify signature
            if (!_signatureVerifier.VerifySignature(response, _config.PublicKeyPem))
                throw new SignatureVerificationException();

            // Verify product match
            if (!string.Equals(response.ProductId, _config.ProductId, StringComparison.OrdinalIgnoreCase))
                throw new ProductMismatchException(_config.ProductId, response.ProductId);

            // Check server-reported status
            if (response.ParsedStatus == LicenseStatus.Expired)
                throw new LicenseActivationException("License key has expired.");

            if (response.ParsedStatus == LicenseStatus.Revoked)
                throw new LicenseActivationException("License key has been revoked.");

            if (response.ParsedStatus == LicenseStatus.Suspended)
                throw new LicenseActivationException("License key has been suspended.");

            // Validate time
            var timeResult = _timeValidator.Validate(response.ServerTime);
            if (!timeResult.IsValid)
                throw new ClockRollbackException(timeResult.RollbackDuration ?? TimeSpan.Zero);

            // Build cached state
            var cachedState = new CachedLicenseState
            {
                LicenseId = response.LicenseId,
                DeviceId = response.DeviceId,
                LicenseKey = licenseKey,
                SignedState = response,
                LastValidationTime = DateTime.UtcNow,
                LastServerTime = response.ServerTime,
                LastLocalTime = DateTime.UtcNow,
                KeyVersion = response.KeyVersion,
                ProductId = response.ProductId,
                IsInOfflineGrace = false,
                OfflineGraceStartTime = null,
                LastNonce = response.Nonce
            };

            // Save to cache
            _cache.Save(cachedState);
            _currentState = cachedState;

            // Load features
            _featureManager.LoadFeatures(response.Features);

            SetStatus(LicenseStatus.Active);
            StartValidationTimer();

            _logger.LogInformation("License activated successfully: {LicenseId}", response.LicenseId);

            return BuildLicenseInfo();
        }
        catch (HttpRequestException ex)
        {
            SetStatus(LicenseStatus.Error);
            throw new LicenseActivationException(
                $"Failed to connect to license server: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates the current license with the server.
    /// </summary>
    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        if (_currentState == null)
        {
            _logger.LogWarning("No license to validate");
            return;
        }

        // First validate locally
        var localOutcome = _validator.Validate(_currentState, _config.ProductId, _config.OfflineGraceHours);
        if (!localOutcome.IsValid && !localOutcome.IsOfflineGrace)
        {
            SetStatus(localOutcome.Status);
            return;
        }

        // Then validate with server
        try
        {
            await ValidateWithServerAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Server validation failed, using cached state");
            // Enter offline grace if not already
            if (!_currentState.IsInOfflineGrace)
            {
                _currentState.IsInOfflineGrace = true;
                _currentState.OfflineGraceStartTime = DateTime.UtcNow;
                _cache.Save(_currentState);
                SetStatus(LicenseStatus.OfflineGrace);
            }
        }
    }

    /// <summary>
    /// Deactivates the license, removing local state and notifying the server.
    /// </summary>
    public async Task DeactivateAsync(CancellationToken cancellationToken = default)
    {
        if (_currentState == null)
        {
            _logger.LogDebug("No license to deactivate");
            return;
        }

        _logger.LogInformation("Deactivating license");

        // Notify server best-effort
        try
        {
            var request = new DeactivationRequest
            {
                LicenseKey = _currentState.LicenseKey,
                DeviceFingerprint = _deviceIdentity.GetFingerprint()
            };
            await SendRequestAsync<object>("api/license/deactivate", request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify server of deactivation");
        }

        // Clear local state
        _cache.Clear();
        _currentState = null;
        _featureManager.ClearFeatures();
        StopValidationTimer();
        SetStatus(LicenseStatus.Unknown);

        _logger.LogInformation("License deactivated");
    }

    /// <summary>
    /// Gets the current license status.
    /// </summary>
    public LicenseStatus GetStatus() => _currentStatus;

    /// <summary>
    /// Gets comprehensive license information.
    /// </summary>
    public LicenseInfo GetLicenseInfo() => BuildLicenseInfo();

    /// <summary>
    /// Checks if a specific feature is enabled.
    /// </summary>
    public bool HasFeature(string featureKey) => _featureManager.HasFeature(featureKey);

    /// <summary>
    /// Gets the license expiry date, if available.
    /// </summary>
    public DateTime? GetExpiry() => _currentState?.SignedState?.ExpiresAt;

    /// <summary>
    /// Gets the current plan name.
    /// </summary>
    public string GetPlan() => _currentState?.SignedState?.Plan ?? string.Empty;

    /// <summary>
    /// Gets the underlying cached state (for advanced scenarios).
    /// </summary>
    internal CachedLicenseState? GetCurrentState() => _currentState;

    #region Private Methods

    private async Task ValidateWithServerAsync(CancellationToken cancellationToken)
    {
        if (_currentState == null) return;

        var fingerprint = _deviceIdentity.GetFingerprint();
        var nonce = Guid.NewGuid().ToString("N");

        var request = new ValidationRequest
        {
            LicenseKey = _currentState.LicenseKey,
            ProductId = _config.ProductId,
            DeviceFingerprint = fingerprint,
            ApplicationVersion = _config.ApplicationVersion,
            Nonce = nonce
        };

        var response = await SendRequestAsync<SignedLicenseResponse>(
            "api/license/validate", request, cancellationToken);

        if (response == null)
            throw new LicenseValidationException("Empty response from license server.");

        // Verify signature
        if (!_signatureVerifier.VerifySignature(response, _config.PublicKeyPem))
            throw new SignatureVerificationException();

        // Verify product match
        if (!string.Equals(response.ProductId, _config.ProductId, StringComparison.OrdinalIgnoreCase))
            throw new ProductMismatchException(_config.ProductId, response.ProductId);

        // Check server status
        if (response.ParsedStatus == LicenseStatus.Expired)
            throw new LicenseValidationException("License has expired.");

        if (response.ParsedStatus == LicenseStatus.Revoked)
            throw new LicenseValidationException("License has been revoked.");

        if (response.ParsedStatus == LicenseStatus.Suspended)
            throw new LicenseValidationException("License has been suspended.");

        // Validate time
        var timeResult = _timeValidator.Validate(response.ServerTime);
        if (!timeResult.IsValid)
            throw new ClockRollbackException(timeResult.RollbackDuration ?? TimeSpan.Zero);

        // Update cached state
        _currentState.SignedState = response;
        _currentState.LastValidationTime = DateTime.UtcNow;
        _currentState.LastServerTime = response.ServerTime;
        _currentState.LastLocalTime = DateTime.UtcNow;
        _currentState.KeyVersion = response.KeyVersion;
        _currentState.IsInOfflineGrace = false;
        _currentState.OfflineGraceStartTime = null;
        _currentState.LastNonce = nonce;

        _cache.Save(_currentState);

        // Update features
        _featureManager.LoadFeatures(response.Features);

        SetStatus(LicenseStatus.Active);
    }

    private async Task<T?> SendRequestAsync<T>(string endpoint, object requestBody, CancellationToken cancellationToken)
    {
        var url = $"{_config.ServerUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(requestBody, options: JsonOptions)
        };

        // Add headers
        request.Headers.Add("X-Product-Id", _config.ProductId);
        request.Headers.Add("X-Product-Code", _config.ProductCode);
        request.Headers.Add("X-App-Version", _config.ApplicationVersion);
        request.Headers.Add("X-SDK-Version", "1.0.0");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private void SetStatus(LicenseStatus newStatus)
    {
        if (_currentStatus != newStatus)
        {
            var oldStatus = _currentStatus;
            _currentStatus = newStatus;
            _logger.LogDebug("License status changed: {OldStatus} -> {NewStatus}", oldStatus, newStatus);
            StatusChanged?.Invoke(this, newStatus);
        }
    }

    private LicenseInfo BuildLicenseInfo()
    {
        var signed = _currentState?.SignedState;
        return new LicenseInfo
        {
            LicenseId = signed?.LicenseId ?? string.Empty,
            LicenseKey = _currentState?.LicenseKey ?? string.Empty,
            ProductId = signed?.ProductId ?? _config.ProductId,
            Status = _currentStatus,
            ExpiryDate = signed?.ExpiresAt,
            Plan = signed?.Plan ?? string.Empty,
            Features = signed?.Features?.AsReadOnly() ?? Array.Empty<string>().AsReadOnly(),
            LastValidation = _currentState?.LastValidationTime,
            DeviceId = _currentState?.DeviceId ?? string.Empty,
            MaxDevices = signed?.MaxDevices ?? 0,
            IsInOfflineGrace = _currentState?.IsInOfflineGrace ?? false,
            OfflineGraceStartTime = _currentState?.OfflineGraceStartTime,
            KeyVersion = signed?.KeyVersion ?? 0,
            ServerTime = signed?.ServerTime
        };
    }

    private void StartValidationTimer()
    {
        StopValidationTimer();

        var interval = TimeSpan.FromMinutes(_config.ValidationIntervalMinutes);
        _validationTimer = new Timer(
            async _ => await OnValidationTimerTickAsync(),
            null,
            interval,
            interval);

        _logger.LogDebug("Validation timer started: interval={Interval}min", _config.ValidationIntervalMinutes);
    }

    private void StopValidationTimer()
    {
        if (_validationTimer != null)
        {
            _validationTimer.Dispose();
            _validationTimer = null;
        }
    }

    private async Task OnValidationTimerTickAsync()
    {
        lock (_lock)
        {
            if (_disposed) return;
        }

        try
        {
            _logger.LogDebug("Background validation triggered");
            await ValidateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Background validation failed");
        }
    }

    #endregion

    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            _disposed = true;
            StopValidationTimer();
        }

        GC.SuppressFinalize(this);
    }
}
