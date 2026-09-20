using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class AdminCreateRequest
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(300)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(200)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
