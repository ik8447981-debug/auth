namespace LicenseClient.SDK.Models;

/// <summary>
/// Request payload for license activation.
/// </summary>
public class ActivationRequest
{
    /// <summary>
    /// The license key to activate.
    /// </summary>
    public string LicenseKey { get; set; } = string.Empty;

    /// <summary>
    /// Target product identifier.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Device fingerprint for binding.
    /// </summary>
    public string DeviceFingerprint { get; set; } = string.Empty;

    /// <summary>
    /// Application version making the request.
    /// </summary>
    public string ApplicationVersion { get; set; } = string.Empty;

    /// <summary>
    /// Operating system version string.
    /// </summary>
    public string OSVersion { get; set; } = string.Empty;
}

/// <summary>
/// Request payload for license validation/revalidation.
/// </summary>
public class ValidationRequest
{
    /// <summary>
    /// The license key being validated.
    /// </summary>
    public string LicenseKey { get; set; } = string.Empty;

    /// <summary>
    /// Target product identifier.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Device fingerprint for verification.
    /// </summary>
    public string DeviceFingerprint { get; set; } = string.Empty;

    /// <summary>
    /// Application version making the request.
    /// </summary>
    public string ApplicationVersion { get; set; } = string.Empty;

    /// <summary>
    /// Cryptographic nonce for replay protection.
    /// </summary>
    public string Nonce { get; set; } = string.Empty;
}

/// <summary>
/// Request payload for license deactivation.
/// </summary>
public class DeactivationRequest
{
    /// <summary>
    /// The license key to deactivate.
    /// </summary>
    public string LicenseKey { get; set; } = string.Empty;

    /// <summary>
    /// Device fingerprint releasing the license.
    /// </summary>
    public string DeviceFingerprint { get; set; } = string.Empty;
}
