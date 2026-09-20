namespace LicensePlatform.Shared.DTOs.Responses;

public class LicensePlanResponse
{
    public Guid PlanId { get; set; }
    public Guid ProductId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public int MaxDevices { get; set; }
    public int OfflineGraceHours { get; set; }
    public int ValidationIntervalMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<Guid> FeatureIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
