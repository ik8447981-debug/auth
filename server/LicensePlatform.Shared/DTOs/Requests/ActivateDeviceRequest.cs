using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class ActivateDeviceRequest
{
    [Required]
    public string LicenseKey { get; set; } = string.Empty;

    [Required]
    public string DeviceFingerprint { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ApplicationVersion { get; set; } = string.Empty;

    [MaxLength(200)]
    public string OSVersion { get; set; } = string.Empty;
}
