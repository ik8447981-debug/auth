namespace LicensePlatform.Shared.DTOs.Responses;

public class ProductFeatureResponse
{
    public Guid FeatureId { get; set; }
    public string FeatureKey { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
