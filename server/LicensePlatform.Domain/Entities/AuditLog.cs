using System;
using System.ComponentModel.DataAnnotations;
using LicensePlatform.Domain.Enums;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Records administrative actions and system events for security auditing and compliance.
    /// </summary>
    public class AuditLog
    {
        /// <summary>Unique identifier for this audit log entry.</summary>
        public Guid AuditLogId { get; set; }

        /// <summary>Foreign key reference to the admin user who performed the action. Null for system-generated events.</summary>
        public Guid? ActorId { get; set; }

        /// <summary>Display name of the actor at the time of the action (denormalized for historical accuracy).</summary>
        [MaxLength(200)]
        public string ActorName { get; set; } = string.Empty;

        /// <summary>The type of action that was performed.</summary>
        public AuditAction Action { get; set; }

        /// <summary>The type of entity or resource that was affected (e.g., Product, License, Customer).</summary>
        [MaxLength(100)]
        public string TargetType { get; set; } = string.Empty;

        /// <summary>The unique identifier of the affected entity (if applicable).</summary>
        [MaxLength(100)]
        public string TargetId { get; set; } = string.Empty;

        /// <summary>Timestamp when the audited action occurred.</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>IP address from which the action was performed.</summary>
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>Outcome of the action (success or failure).</summary>
        public string Result { get; set; } = "success";

        /// <summary>Additional metadata about the action serialized as a JSON string.</summary>
        public string Metadata { get; set; } = "{}";

        /// <summary>Unique request identifier for correlating related events across services.</summary>
        [MaxLength(100)]
        public string RequestId { get; set; } = string.Empty;

        /// <summary>Navigation property to the admin user who performed the action.</summary>
        public AdminUser? Actor { get; set; }
    }
}
