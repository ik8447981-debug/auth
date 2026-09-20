using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class ExtendLicenseRequest
{
    [Required]
    public DateTime NewExpiryDate { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
