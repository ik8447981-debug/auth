namespace LicensePlatform.Shared.DTOs;

public class OfflineValidationState
{
    public Guid LicenseId { get; set; }
    public Guid ProductId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public List<string> Features { get; set; } = new();
    public DateTime LastValidationTime { get; set; }
    public DateTime LastServerTime { get; set; }
    public int GracePeriodHours { get; set; }
    public string SignedState { get; set; } = string.Empty;
}
