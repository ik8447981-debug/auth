namespace LicensePlatform.Shared.DTOs.Responses;

public class LicenseActivationResponse
{
    public bool Success { get; set; }
    public Guid? LicenseId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public List<string> Features { get; set; } = new();
    public Guid? DeviceId { get; set; }
    public string SignedLicenseState { get; set; } = string.Empty;
    public DateTime ServerTime { get; set; }
    public int KeyVersion { get; set; }
    public string Nonce { get; set; } = string.Empty;
}
