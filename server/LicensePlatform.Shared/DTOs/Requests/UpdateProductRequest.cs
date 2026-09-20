namespace LicensePlatform.Shared.DTOs.Requests;

public class UpdateProductRequest
{
    public string? ProductName { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
}
