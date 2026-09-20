using System.Collections.Concurrent;

namespace LicenseClient.SDK.Features;

/// <summary>
/// Manages feature entitlements from signed license responses.
/// Features are sourced from the server-signed response, never self-declared.
/// </summary>
public class FeatureManager : IFeatureManager
{
    private readonly ConcurrentDictionary<string, bool> _features = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if a specific feature is enabled.
    /// Feature keys are case-insensitive.
    /// </summary>
    /// <param name="featureKey">The feature key to check.</param>
    /// <returns>True if the feature is enabled; false otherwise.</returns>
    public bool HasFeature(string featureKey)
    {
        if (string.IsNullOrWhiteSpace(featureKey))
            return false;

        return _features.ContainsKey(featureKey);
    }

    /// <summary>
    /// Gets all enabled feature keys.
    /// </summary>
    /// <returns>Read-only list of feature keys.</returns>
    public IReadOnlyList<string> GetAllFeatures()
    {
        return _features.Keys.ToArray();
    }

    /// <summary>
    /// Loads features from a signed license response.
    /// Replaces any previously loaded features.
    /// </summary>
    /// <param name="features">Feature keys from the signed response.</param>
    public void LoadFeatures(IEnumerable<string> features)
    {
        _features.Clear();

        if (features == null)
            return;

        foreach (var feature in features)
        {
            if (!string.IsNullOrWhiteSpace(feature))
            {
                _features.TryAdd(feature, true);
            }
        }
    }

    /// <summary>
    /// Clears all loaded features.
    /// </summary>
    public void ClearFeatures()
    {
        _features.Clear();
    }

    /// <summary>
    /// Gets the count of loaded features.
    /// </summary>
    public int FeatureCount => _features.Count;
}
