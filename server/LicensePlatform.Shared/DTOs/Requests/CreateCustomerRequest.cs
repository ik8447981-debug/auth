using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class CreateCustomerRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(300)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Notes { get; set; } = string.Empty;
}
