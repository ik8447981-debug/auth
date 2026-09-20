using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class BulkGenerateLicenseRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid PlanId { get; set; }

    [Range(1, 1000)]
    public int Count { get; set; }

    public Guid? CustomerId { get; set; }

    [Range(1, 100)]
    public int MaxDevices { get; set; }

    public List<Guid> FeatureIds { get; set; } = new();
}
