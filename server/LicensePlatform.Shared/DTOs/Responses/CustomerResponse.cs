namespace LicensePlatform.Shared.DTOs.Responses;

public class CustomerResponse
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int LicenseCount { get; set; }
}
