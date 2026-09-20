using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for AuditLog-specific data access operations.
    /// </summary>
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        /// <summary>
        /// Gets all audit log entries for a specific actor (user or system).
        /// </summary>
        Task<IReadOnlyList<AuditLog>> GetByActorAsync(string actorName);

        /// <summary>
        /// Gets all audit log entries targeting a specific entity.
        /// </summary>
        Task<IReadOnlyList<AuditLog>> GetByTargetAsync(string targetType, string targetId);

        /// <summary>
        /// Gets all audit log entries within a specified date range.
        /// </summary>
        Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to);
    }
}
