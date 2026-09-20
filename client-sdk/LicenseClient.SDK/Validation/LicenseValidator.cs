using LicenseClient.SDK.Caching;
using LicenseClient.SDK.Models;

namespace LicenseClient.SDK.Validation;

/// <summary>
/// Interface for license validation.
/// </summary>
public interface ILicenseValidator
{
    /// <summary>
    /// Validates a cached license against all client-side rules.
    /// </summary>
    ValidationOutcome Validate(CachedLicenseState cachedState, string expectedProductId, int offlineGraceHours);
}

/// <summary>
/// Result of a validation check.
/// </summary>
public class ValidationOutcome
{
    /// <summary>
    /// Whether the license is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// The resolved license status.
    /// </summary>
    public LicenseStatus Status { get; set; } = LicenseStatus.Unknown;

    /// <summary>
    /// Reason for failure, if applicable.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Whether offline grace period is active.
    /// </summary>
    public bool IsOfflineGrace { get; set; }

    /// <summary>
    /// Creates a successful validation outcome.
    /// </summary>
    public static ValidationOutcome Success(LicenseStatus status) => new()
    {
        IsValid = true,
        Status = status
    };

    /// <summary>
    /// Creates a failed validation outcome.
    /// </summary>
    public static ValidationOutcome Failed(LicenseStatus status, string reason) => new()
    {
        IsValid = false,
        Status = status,
        FailureReason = reason
    };
}

/// <summary>
/// Client-side license validation logic. Performs all checks that can
/// be done without contacting the server.
/// </summary>
public class LicenseValidator : ILicenseValidator
{
    private readonly ISignatureVerifier _signatureVerifier;
    private readonly ServerTimeValidator _timeValidator;
    private readonly string _publicKeyPem;

    /// <summary>
    /// Creates a new LicenseValidator.
    /// </summary>
    /// <param name="signatureVerifier">Signature verification service.</param>
    /// <param name="timeValidator">Server time validation service.</param>
    /// <param name="publicKeyPem">RSA public key for signature verification.</param>
    public LicenseValidator(
        ISignatureVerifier signatureVerifier,
        ServerTimeValidator timeValidator,
        string publicKeyPem)
    {
        _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
        _timeValidator = timeValidator ?? throw new ArgumentNullException(nameof(timeValidator));
        _publicKeyPem = publicKeyPem ?? throw new ArgumentNullException(nameof(publicKeyPem));
    }

    /// <summary>
    /// Validates a cached license against all client-side rules.
    /// Checks are performed in order of severity.
    /// </summary>
    /// <param name="cachedState">The cached license state to validate.</param>
    /// <param name="expectedProductId">The product ID this application expects.</param>
    /// <param name="offlineGraceHours">How many hours offline grace is allowed.</param>
    /// <returns>Validation outcome with status and any failure reason.</returns>
    public ValidationOutcome Validate(CachedLicenseState cachedState, string expectedProductId, int offlineGraceHours)
    {
        if (cachedState == null)
            return ValidationOutcome.Failed(LicenseStatus.Unknown, "No cached license state.");

        var signedResponse = cachedState.SignedState;
        if (signedResponse == null)
            return ValidationOutcome.Failed(LicenseStatus.Error, "Missing signed license state.");

        // Check 1: Product ID isolation (CRITICAL)
        // This prevents a license for Product A from being used with Product B
        if (!string.Equals(signedResponse.ProductId, expectedProductId, StringComparison.OrdinalIgnoreCase))
            return ValidationOutcome.Failed(LicenseStatus.ProductMismatch,
                $"Product mismatch: expected '{expectedProductId}', got '{signedResponse.ProductId}'.");

        // Also check the cache-level product ID
        if (!string.Equals(cachedState.ProductId, expectedProductId, StringComparison.OrdinalIgnoreCase))
            return ValidationOutcome.Failed(LicenseStatus.ProductMismatch,
                $"Cache product mismatch: expected '{expectedProductId}', got '{cachedState.ProductId}'.");

        // Check 2: Cryptographic signature verification
        if (!_signatureVerifier.VerifySignature(signedResponse, _publicKeyPem))
            return ValidationOutcome.Failed(LicenseStatus.Error,
                "Signature verification failed. License data may be tampered.");

        // Check 3: Key version compatibility
        // If the key version is higher than what we support, we can't verify
        if (signedResponse.KeyVersion < 1)
            return ValidationOutcome.Failed(LicenseStatus.Error,
                "Invalid key version in signed response.");

        // Check 4: License status
        var parsedStatus = signedResponse.ParsedStatus;
        if (parsedStatus == LicenseStatus.Expired)
            return ValidationOutcome.Failed(LicenseStatus.Expired, "License has expired.");

        if (parsedStatus == LicenseStatus.Suspended)
            return ValidationOutcome.Failed(LicenseStatus.Suspended, "License has been suspended.");

        if (parsedStatus == LicenseStatus.Revoked)
            return ValidationOutcome.Failed(LicenseStatus.Revoked, "License has been revoked.");

        if (parsedStatus != LicenseStatus.Active)
            return ValidationOutcome.Failed(parsedStatus, $"Unexpected license status: {signedResponse.Status}.");

        // Check 5: Expiry date
        if (signedResponse.ExpiresAt.HasValue)
        {
            var expiry = signedResponse.ExpiresAt.Value;
            var now = DateTime.UtcNow;

            if (now > expiry)
            {
                // License expired - check offline grace
                return CheckOfflineGrace(cachedState, offlineGraceHours, now);
            }
        }

        // Check 6: Clock rollback detection
        var timeResult = _timeValidator.Validate(signedResponse.ServerTime);
        if (!timeResult.IsValid && timeResult.ClockRollbackDetected)
        {
            return ValidationOutcome.Failed(LicenseStatus.Error,
                $"Clock rollback detected: {timeResult.Message}");
        }

        // Check 7: Device binding
        // Note: Full device verification happens server-side
        // Client-side we just check the field exists
        if (string.IsNullOrWhiteSpace(signedResponse.DeviceId))
            return ValidationOutcome.Failed(LicenseStatus.Error,
                "Missing device binding in license.");

        // All checks passed
        return ValidationOutcome.Success(LicenseStatus.Active);
    }

    /// <summary>
    /// Checks whether offline grace period allows continued use after expiry.
    /// </summary>
    private ValidationOutcome CheckOfflineGrace(
        CachedLicenseState cachedState, int offlineGraceHours, DateTime now)
    {
        // If already in offline grace period
        if (cachedState.IsInOfflineGrace && cachedState.OfflineGraceStartTime.HasValue)
        {
            var graceElapsed = now - cachedState.OfflineGraceStartTime.Value;
            if (graceElapsed.TotalHours <= offlineGraceHours)
            {
                return new ValidationOutcome
                {
                    IsValid = true,
                    Status = LicenseStatus.OfflineGrace,
                    IsOfflineGrace = true,
                    FailureReason = null
                };
            }
            else
            {
                return ValidationOutcome.Failed(LicenseStatus.Expired,
                    $"Offline grace period expired ({offlineGraceHours}h).");
            }
        }

        // Not in offline grace - license is simply expired
        return ValidationOutcome.Failed(LicenseStatus.Expired, "License has expired.");
    }
}
