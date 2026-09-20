using System;
using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Stores system-wide configuration settings as key-value pairs.
    /// </summary>
    public class SystemSetting
    {
        /// <summary>Unique identifier for this setting record.</summary>
        public Guid SystemSettingId { get; set; }

        /// <summary>Unique key identifying this setting (e.g., smtp_host, license_grace_hours).</summary>
        [Required]
        [MaxLength(200)]
        public string SettingKey { get; set; } = string.Empty;

        /// <summary>Value of the setting stored as a string (may be JSON for complex values).</summary>
        [Required]
        public string SettingValue { get; set; } = string.Empty;

        /// <summary>Human-readable description explaining what this setting controls.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Timestamp when this setting was last updated.</summary>
        public DateTime UpdatedAt { get; set; }
    }
}
