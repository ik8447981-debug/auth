namespace LicensePlatform.Shared.Security;

public class SignedLicenseResponse
{
    public Guid LicenseId { get; set; }
    public Guid ProductId { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid? DeviceId { get; set; }
    public List<string> Features { get; set; } = new();
    public int MaxDevices { get; set; }
    public DateTime ServerTime { get; set; }
    public int LicenseVersion { get; set; }
    public int KeyVersion { get; set; }
    public string Nonce { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}
