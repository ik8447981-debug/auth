using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class RevokeLicenseRequest
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
