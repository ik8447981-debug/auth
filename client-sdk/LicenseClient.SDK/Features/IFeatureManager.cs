namespace LicenseClient.SDK.Features;

/// <summary>
/// Interface for feature entitlement management.
/// </summary>
public interface IFeatureManager
{
    /// <summary>
    /// Checks if a specific feature is enabled in the current license.
    /// </summary>
    bool HasFeature(string featureKey);

    /// <summary>
    /// Gets all enabled feature keys.
    /// </summary>
    IReadOnlyList<string> GetAllFeatures();

    /// <summary>
    /// Loads features from a signed license response.
    /// </summary>
    void LoadFeatures(IEnumerable<string> features);

    /// <summary>
    /// Clears all loaded features.
    /// </summary>
    void ClearFeatures();
}
