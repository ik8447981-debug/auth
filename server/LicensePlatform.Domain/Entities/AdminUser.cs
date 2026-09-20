using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LicensePlatform.Domain.Enums;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Represents an administrative user who can access the license management platform.
    /// </summary>
    public class AdminUser
    {
        /// <summary>Unique identifier for the admin user.</summary>
        public Guid AdminUserId { get; set; }

        /// <summary>Unique username used for authentication.</summary>
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        /// <summary>Unique email address for the admin user.</summary>
        [Required]
        [MaxLength(300)]
        public string Email { get; set; } = string.Empty;

        /// <summary>Hashed password for secure authentication.</summary>
        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Administrative role determining access permissions.</summary>
        public AdminRole Role { get; set; }

        /// <summary>Indicates whether this admin account is currently active.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Timestamp when the admin account was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Timestamp of the admin's most recent successful login.</summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>Number of consecutive failed login attempts (used for lockout logic).</summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>Timestamp until which the account is locked due to too many failed attempts.</summary>
        public DateTime? LockedUntil { get; set; }

        /// <summary>Navigation property for audit logs created by this admin user.</summary>
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
