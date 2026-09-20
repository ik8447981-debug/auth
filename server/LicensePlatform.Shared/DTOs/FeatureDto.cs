namespace LicensePlatform.Shared.DTOs;

public class FeatureDto
{
    public Guid FeatureId { get; set; }
    public string FeatureKey { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
}
