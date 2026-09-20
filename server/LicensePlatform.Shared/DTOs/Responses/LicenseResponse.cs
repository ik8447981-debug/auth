namespace LicensePlatform.Shared.DTOs.Responses;

public class LicenseResponse
{
    public Guid LicenseId { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int MaxDevices { get; set; }
    public int ActiveDeviceCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
}
