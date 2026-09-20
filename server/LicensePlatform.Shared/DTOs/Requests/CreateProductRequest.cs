using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class CreateProductRequest
{
    [Required]
    [MaxLength(100)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;
}
