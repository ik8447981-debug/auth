namespace LicensePlatform.Shared.DTOs.Responses;

public class AnalyticsResponse
{
    public int TotalProducts { get; set; }
    public int TotalLicenses { get; set; }
    public int ActiveLicenses { get; set; }
    public int ExpiredLicenses { get; set; }
    public int RevokedLicenses { get; set; }
    public int ActiveDevices { get; set; }
    public int ActivationsToday { get; set; }
    public int ValidationsToday { get; set; }
    public int ApiErrorsToday { get; set; }
}
