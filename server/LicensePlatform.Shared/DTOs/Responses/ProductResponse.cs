namespace LicensePlatform.Shared.DTOs.Responses;

public class ProductResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CurrentVersion { get; set; } = string.Empty;
    public string LatestVersion { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int LicenseCount { get; set; }
    public int CustomerCount { get; set; }
    public int ActiveDeviceCount { get; set; }
}
