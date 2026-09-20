using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class DeactivateDeviceRequest
{
    [Required]
    public string LicenseKey { get; set; } = string.Empty;

    [Required]
    public string DeviceFingerprint { get; set; } = string.Empty;
}
