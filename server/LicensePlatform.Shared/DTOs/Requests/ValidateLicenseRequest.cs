using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class ValidateLicenseRequest
{
    [Required]
    public string LicenseKey { get; set; } = string.Empty;

    [Required]
    public string DeviceFingerprint { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ApplicationVersion { get; set; } = string.Empty;

    [Required]
    public string Nonce { get; set; } = string.Empty;
}
