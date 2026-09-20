using LicensePlatform.Application.Interfaces;
using LicensePlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LicensePlatform.Api.Controllers;

[ApiController]
[Route("api/v1/admin/audit-logs")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(IAuditService auditService, ILogger<AuditLogsController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAuditLogs(
        [FromQuery] string? action,
        [FromQuery] string? targetType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        AuditAction? auditAction = null;
        if (!string.IsNullOrEmpty(action) && Enum.TryParse<AuditAction>(action, true, out var parsed))
            auditAction = parsed;

        var result = _auditService.GetAuditLogs(page, pageSize, auditAction, targetType, startDate, endDate);
        return Ok(result);
    }

    [HttpGet("target/{targetType}/{targetId}")]
    public IActionResult GetByTarget(string targetType, string targetId)
    {
        var logs = _auditService.GetAuditLogsByTarget(targetType, targetId);
        return Ok(logs);
    }
}
