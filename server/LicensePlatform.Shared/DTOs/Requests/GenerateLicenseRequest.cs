using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class GenerateLicenseRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public Guid PlanId { get; set; }

    [Required]
    public string LicenseType { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [Range(1, 100)]
    public int MaxDevices { get; set; }

    public List<Guid> FeatureIds { get; set; } = new();
}
