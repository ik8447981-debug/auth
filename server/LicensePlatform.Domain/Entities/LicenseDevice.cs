using System;
using System.ComponentModel.DataAnnotations;
using LicensePlatform.Domain.Enums;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Represents a client device registered to use a specific license.
    /// </summary>
    public class LicenseDevice
    {
        /// <summary>Unique identifier for this device registration record.</summary>
        public Guid LicenseDeviceId { get; set; }

        /// <summary>Foreign key reference to the associated license.</summary>
        public Guid LicenseId { get; set; }

        /// <summary>Foreign key reference to the product being used on this device.</summary>
        public Guid ProductId { get; set; }

        /// <summary>Foreign key reference to the customer who owns this device registration.</summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Unique device fingerprint hash used for device identification.
        /// Generated from hardware identifiers for consistency across reinstalls.
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string DeviceFingerprint { get; set; } = string.Empty;

        /// <summary>User-assigned or auto-detected name for the device.</summary>
        [MaxLength(200)]
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>Timestamp when this device was first seen by the system.</summary>
        public DateTime FirstSeen { get; set; }

        /// <summary>Timestamp when this device last communicated with the license server.</summary>
        public DateTime LastSeen { get; set; }

        /// <summary>Timestamp when the device was activated for the license.</summary>
        public DateTime ActivationDate { get; set; }

        /// <summary>Current status of the device registration.</summary>
        public DeviceStatus Status { get; set; }

        /// <summary>Version of the client application running on this device.</summary>
        [MaxLength(50)]
        public string ApplicationVersion { get; set; } = string.Empty;

        /// <summary>Operating system version detected on this device.</summary>
        [MaxLength(100)]
        public string OSVersion { get; set; } = string.Empty;

        /// <summary>Navigation property to the associated license.</summary>
        public License License { get; set; } = null!;
    }
}
