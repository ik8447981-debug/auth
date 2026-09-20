namespace LicensePlatform.Shared.Security;

public class LicenseVerificationResult
{
    public bool IsValid { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
