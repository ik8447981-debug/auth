namespace LicensePlatform.Shared.DTOs.Responses;

public class AuditLogResponse
{
    public Guid LogId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}
