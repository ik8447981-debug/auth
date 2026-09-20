namespace LicensePlatform.Shared.DTOs.Responses;

public class LicenseDeviceResponse
{
    public Guid DeviceId { get; set; }
    public string DeviceFingerprint { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public DateTime ActivationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ApplicationVersion { get; set; } = string.Empty;
    public string OSVersion { get; set; } = string.Empty;
}
