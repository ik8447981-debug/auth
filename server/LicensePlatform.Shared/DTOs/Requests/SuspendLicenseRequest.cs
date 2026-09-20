using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class SuspendLicenseRequest
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
