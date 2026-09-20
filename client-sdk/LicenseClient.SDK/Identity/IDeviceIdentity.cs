namespace LicenseClient.SDK.Identity;

/// <summary>
/// Interface for device identity generation.
/// </summary>
public interface IDeviceIdentity
{
    /// <summary>
    /// Generates a stable device fingerprint hash.
    /// </summary>
    string GetFingerprint();

    /// <summary>
    /// Gets the raw signals used for fingerprinting (for debugging).
    /// </summary>
    IReadOnlyDictionary<string, string> GetSignals();
}
