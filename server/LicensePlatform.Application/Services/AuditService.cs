using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LicensePlatform.Application.Interfaces;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Infrastructure.Data;
using LicensePlatform.Shared.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly LicensePlatformDbContext _context;

        public AuditService(LicensePlatformDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Log(
            AuditAction action,
            string targetType,
            string targetId,
            string? actorId,
            string? actorName,
            string? ipAddress,
            string? result,
            string? metadata,
            string? requestId)
        {
            var auditLog = new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                Action = action,
                TargetType = targetType,
                TargetId = targetId,
                ActorName = actorName ?? "System",
                IpAddress = ipAddress ?? string.Empty,
                Result = result ?? string.Empty,
                Metadata = metadata ?? string.Empty,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            _context.SaveChanges();
        }

        public PaginatedResponse<AuditLogResponse> GetAuditLogs(
            int page, int pageSize, AuditAction? action, string? targetType, DateTime? from, DateTime? to)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _context.AuditLogs.AsQueryable();

            if (action.HasValue)
                query = query.Where(l => l.Action == action.Value);

            if (!string.IsNullOrWhiteSpace(targetType))
                query = query.Where(l => l.TargetType == targetType);

            if (from.HasValue)
                query = query.Where(l => l.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.Timestamp <= to.Value);

            query = query.OrderByDescending(l => l.Timestamp);

            int totalCount = query.Count();

            var logs = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = logs.Select(l => new AuditLogResponse
            {
                LogId = l.AuditLogId,
                ActorName = l.ActorName,
                Action = l.Action.ToString(),
                TargetType = l.TargetType,
                TargetId = l.TargetId,
                Timestamp = l.Timestamp,
                IpAddress = l.IpAddress,
                Result = l.Result,
                Metadata = ParseMetadata(l.Metadata)
            }).ToList();

            return new PaginatedResponse<AuditLogResponse>(items, totalCount, page, pageSize);
        }

        public List<AuditLogResponse> GetAuditLogsByTarget(string targetType, string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetType))
                throw new ArgumentException("Target type cannot be empty.");

            if (string.IsNullOrWhiteSpace(targetId))
                throw new ArgumentException("Target ID cannot be empty.");

            return _context.AuditLogs
                .Where(l => l.TargetType == targetType && l.TargetId == targetId)
                .OrderByDescending(l => l.Timestamp)
                .Take(100)
                .Select(l => new AuditLogResponse
                {
                    LogId = l.AuditLogId,
                    ActorName = l.ActorName,
                    Action = l.Action.ToString(),
                    TargetType = l.TargetType,
                    TargetId = l.TargetId,
                    Timestamp = l.Timestamp,
                    IpAddress = l.IpAddress,
                    Result = l.Result,
                    Metadata = null
                })
                .ToList();
        }

        private static Dictionary<string, object>? ParseMetadata(string? metadataJson)
        {
            if (string.IsNullOrWhiteSpace(metadataJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson);
            }
            catch
            {
                return new Dictionary<string, object> { ["raw"] = metadataJson };
            }
        }
    }
}
