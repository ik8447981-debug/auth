using System;
using System.ComponentModel.DataAnnotations;
using LicensePlatform.Domain.Enums;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Records each license validation attempt made by a client device.
    /// Used for auditing, debugging, and tracking license usage patterns.
    /// </summary>
    public class ValidationEvent
    {
        /// <summary>Unique identifier for this validation event.</summary>
        public Guid ValidationEventId { get; set; }

        /// <summary>Foreign key reference to the license being validated.</summary>
        public Guid LicenseId { get; set; }

        /// <summary>Foreign key reference to the product being validated against.</summary>
        public Guid ProductId { get; set; }

        /// <summary>Device fingerprint of the client requesting validation.</summary>
        [Required]
        [MaxLength(128)]
        public string DeviceFingerprint { get; set; } = string.Empty;

        /// <summary>Type of validation event (activation, validation, heartbeat, deactivation).</summary>
        public ValidationEventType EventType { get; set; }

        /// <summary>Indicates whether the validation attempt was successful.</summary>
        public bool Success { get; set; }

        /// <summary>Error code returned if the validation failed (null on success).</summary>
        [MaxLength(50)]
        public string? ErrorCode { get; set; }

        /// <summary>Server-side timestamp when this event was recorded.</summary>
        public DateTime ServerTimestamp { get; set; }

        /// <summary>Client-side timestamp from the validation request (may differ due to clock skew).</summary>
        public DateTime? ClientTimestamp { get; set; }

        /// <summary>IP address of the client making the validation request.</summary>
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>User-Agent string from the client HTTP request.</summary>
        [MaxLength(500)]
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>Navigation property to the associated license.</summary>
        public License License { get; set; } = null!;
    }
}
