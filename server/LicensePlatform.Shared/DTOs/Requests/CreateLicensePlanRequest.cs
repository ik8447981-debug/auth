using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class CreateLicensePlanRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PlanName { get; set; } = string.Empty;

    [Range(1, 3650)]
    public int DurationDays { get; set; }

    [Range(1, 100)]
    public int MaxDevices { get; set; }

    [Range(0, 8760)]
    public int OfflineGraceHours { get; set; }

    [Range(1, 1440)]
    public int ValidationIntervalMinutes { get; set; }

    public List<Guid> FeatureIds { get; set; } = new();
}
