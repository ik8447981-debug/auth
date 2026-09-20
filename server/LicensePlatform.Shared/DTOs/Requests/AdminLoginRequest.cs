using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Shared.DTOs.Requests;

public class AdminLoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
