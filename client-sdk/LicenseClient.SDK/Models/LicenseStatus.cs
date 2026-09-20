namespace LicenseClient.SDK.Models;

/// <summary>
/// Represents the current status of a license.
/// </summary>
public enum LicenseStatus
{
    Unknown = 0,
    Activating = 1,
    Active = 2,
    Expired = 3,
    Suspended = 4,
    Revoked = 5,
    OfflineGrace = 6,
    Error = 7,
    ProductMismatch = 8
}

/// <summary>
/// Contains comprehensive information about a license.
/// </summary>
public class LicenseInfo
{
    /// <summary>
    /// Unique identifier for this license.
    /// </summary>
    public string LicenseId { get; set; } = string.Empty;

    /// <summary>
    /// The license key (may be masked in output).
    /// </summary>
    public string LicenseKey { get; set; } = string.Empty;

    /// <summary>
    /// The product this license is for.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the license.
    /// </summary>
    public LicenseStatus Status { get; set; } = LicenseStatus.Unknown;

    /// <summary>
    /// When the license expires, if applicable.
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// The plan/tier name (e.g., "Pro", "Enterprise").
    /// </summary>
    public string Plan { get; set; } = string.Empty;

    /// <summary>
    /// List of feature keys this license enables.
    /// </summary>
    public IReadOnlyList<string> Features { get; set; } = Array.Empty<string>();

    /// <summary>
    /// When the license was last validated against the server.
    /// </summary>
    public DateTime? LastValidation { get; set; }

    /// <summary>
    /// The device fingerprint this license is bound to.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of devices allowed.
    /// </summary>
    public int MaxDevices { get; set; } = 1;

    /// <summary>
    /// Whether the license is currently in offline grace period.
    /// </summary>
    public bool IsInOfflineGrace { get; set; }

    /// <summary>
    /// When offline grace period started, if applicable.
    /// </summary>
    public DateTime? OfflineGraceStartTime { get; set; }

    /// <summary>
    /// The key version for rotation tracking.
    /// </summary>
    public int KeyVersion { get; set; }

    /// <summary>
    /// Server-reported time from last validation.
    /// </summary>
    public DateTime? ServerTime { get; set; }

    /// <summary>
    /// Masked version of the license key for safe display.
    /// </summary>
    public string MaskedKey => string.IsNullOrEmpty(LicenseKey)
        ? string.Empty
        : MaskLicenseKey(LicenseKey);

    private static string MaskLicenseKey(string key)
    {
        if (key.Length <= 8)
            return new string('*', key.Length);

        var prefix = key[..4];
        var suffix = key[^4..];
        var maskedLength = key.Length - 8;
        return $"{prefix}{new string('*', maskedLength)}{suffix}";
    }

    public override string ToString()
    {
        return $"License {MaskedKey} | {ProductId} | {Plan} | {Status}" +
               (ExpiryDate.HasValue ? $" | Expires: {ExpiryDate:yyyy-MM-dd}" : string.Empty);
    }
}
