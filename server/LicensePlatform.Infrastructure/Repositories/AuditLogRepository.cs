using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for AuditLog data access.
    /// </summary>
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(LicensePlatformDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<AuditLog>> GetByActorAsync(string actorName)
        {
            return await _dbSet
                .Where(a => a.ActorName == actorName)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<AuditLog>> GetByTargetAsync(string targetType, string targetId)
        {
            return await _dbSet
                .Where(a => a.TargetType == targetType && a.TargetId == targetId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _dbSet
                .Where(a => a.Timestamp >= from && a.Timestamp <= to)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}
