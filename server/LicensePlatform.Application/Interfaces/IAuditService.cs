using System;
using System.Collections.Generic;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Shared.DTOs.Responses;

namespace LicensePlatform.Application.Interfaces
{
    public interface IAuditService
    {
        void Log(AuditAction action, string targetType, string targetId, string? actorId, string? actorName, string? ipAddress, string? result, string? metadata, string? requestId);
        PaginatedResponse<AuditLogResponse> GetAuditLogs(int page, int pageSize, AuditAction? action, string? targetType, DateTime? from, DateTime? to);
        List<AuditLogResponse> GetAuditLogsByTarget(string targetType, string targetId);
    }
}
