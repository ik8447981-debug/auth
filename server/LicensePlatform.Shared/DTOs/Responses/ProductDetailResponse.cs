namespace LicensePlatform.Shared.DTOs.Responses;

public class ProductDetailResponse : ProductResponse
{
    public List<ProductFeatureResponse> Features { get; set; } = new();
    public List<LicensePlanResponse> Plans { get; set; } = new();
    public List<ProductVersionDto> Versions { get; set; } = new();
}

public class ProductVersionDto
{
    public Guid VersionId { get; set; }
    public string VersionNumber { get; set; } = string.Empty;
    public DateTime ReleasedAt { get; set; }
    public bool IsLatest { get; set; }
}
