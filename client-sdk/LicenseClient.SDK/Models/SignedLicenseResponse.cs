namespace LicenseClient.SDK.Models;

/// <summary>
/// Server-signed license response. This is the core data structure
/// exchanged between the server and client. All fields are signed.
/// </summary>
public class SignedLicenseResponse
{
    /// <summary>
    /// Unique license identifier.
    /// </summary>
    public string LicenseId { get; set; } = string.Empty;

    /// <summary>
    /// Product this license is for.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Customer identifier.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// License status as reported by the server.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Plan/tier name.
    /// </summary>
    public string Plan { get; set; } = string.Empty;

    /// <summary>
    /// When the license was issued.
    /// </summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>
    /// When the license expires, null if perpetual.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Device fingerprint the license is bound to.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// List of enabled feature keys.
    /// </summary>
    public List<string> Features { get; set; } = new();

    /// <summary>
    /// Maximum concurrent devices allowed.
    /// </summary>
    public int MaxDevices { get; set; } = 1;

    /// <summary>
    /// Server timestamp when response was generated.
    /// </summary>
    public DateTime ServerTime { get; set; }

    /// <summary>
    /// License format version for forward compatibility.
    /// </summary>
    public int LicenseVersion { get; set; } = 1;

    /// <summary>
    /// Key version for key rotation tracking.
    /// </summary>
    public int KeyVersion { get; set; } = 1;

    /// <summary>
    /// Cryptographic nonce to prevent replay attacks.
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// RSA signature over all other fields.
    /// </summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Returns the status as a parsed enum value.
    /// </summary>
    public LicenseStatus ParsedStatus => Status?.ToLowerInvariant() switch
    {
        "active" => Models.LicenseStatus.Active,
        "expired" => Models.LicenseStatus.Expired,
        "suspended" => Models.LicenseStatus.Suspended,
        "revoked" => Models.LicenseStatus.Revoked,
        _ => Models.LicenseStatus.Unknown
    };
}
