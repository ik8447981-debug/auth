namespace LicensePlatform.Shared.DTOs.Responses;

public class LicenseValidationResponse
{
    public bool Success { get; set; }
    public Guid? LicenseId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public List<string> Features { get; set; } = new();
    public DateTime ServerTime { get; set; }
    public int KeyVersion { get; set; }
    public string Nonce { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public bool IsValid { get; set; }
}
