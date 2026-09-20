namespace LicenseClient.SDK.Caching;

using LicenseClient.SDK.Models;

/// <summary>
/// Interface for local license state persistence.
/// </summary>
public interface ILicenseCache
{
    /// <summary>
    /// Loads the cached license state from disk.
    /// Returns null if no cache exists or cache is corrupted.
    /// </summary>
    CachedLicenseState? Load();

    /// <summary>
    /// Saves license state to disk with DPAPI protection.
    /// </summary>
    void Save(CachedLicenseState state);

    /// <summary>
    /// Clears the cached license state.
    /// </summary>
    void Clear();

    /// <summary>
    /// Checks whether a valid cache exists on disk.
    /// </summary>
    bool IsValid();
}

/// <summary>
/// Represents persisted license state.
/// </summary>
public class CachedLicenseState
{
    /// <summary>
    /// License identifier from the server.
    /// </summary>
    public string LicenseId { get; set; } = string.Empty;

    /// <summary>
    /// Device fingerprint at time of activation.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// The license key used for activation.
    /// </summary>
    public string LicenseKey { get; set; } = string.Empty;

    /// <summary>
    /// The full signed response from the server.
    /// </summary>
    public SignedLicenseResponse SignedState { get; set; } = new();

    /// <summary>
    /// When the license was last successfully validated.
    /// </summary>
    public DateTime LastValidationTime { get; set; }

    /// <summary>
    /// Server time reported during last validation.
    /// </summary>
    public DateTime LastServerTime { get; set; }

    /// <summary>
    /// Local time when last validation occurred (for clock rollback detection).
    /// </summary>
    public DateTime LastLocalTime { get; set; }

    /// <summary>
    /// Key version for key rotation tracking.
    /// </summary>
    public int KeyVersion { get; set; }

    /// <summary>
    /// Product identifier this cache belongs to.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user is currently in offline grace period.
    /// </summary>
    public bool IsInOfflineGrace { get; set; }

    /// <summary>
    /// When offline grace period started.
    /// </summary>
    public DateTime? OfflineGraceStartTime { get; set; }

    /// <summary>
    /// Nonce from the last validation request.
    /// </summary>
    public string LastNonce { get; set; } = string.Empty;
}
