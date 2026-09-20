namespace LicensePlatform.Shared.DTOs.Responses;

using LicensePlatform.Shared.DTOs;

public class LicenseDetailResponse : LicenseResponse
{
    public List<FeatureDto> Features { get; set; } = new();
    public List<LicenseDeviceResponse> Devices { get; set; } = new();
    public List<AuditLogResponse> ValidationHistory { get; set; } = new();
}
