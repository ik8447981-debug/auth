using System;
using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Represents an external API client registered to interact with the license management system.
    /// Used for integration with third-party applications and services.
    /// </summary>
    public class ApiClient
    {
        /// <summary>Unique identifier for this API client registration.</summary>
        public Guid ApiClientId { get; set; }

        /// <summary>Display name identifying the external application or service.</summary>
        [Required]
        [MaxLength(200)]
        public string ClientName { get; set; } = string.Empty;

        /// <summary>Unique API key used for client identification in requests.</summary>
        [Required]
        [MaxLength(100)]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>Secret key used for request signing and authentication.</summary>
        [Required]
        [MaxLength(500)]
        public string ApiSecret { get; set; } = string.Empty;

        /// <summary>Indicates whether this API client is currently authorized to make requests.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Timestamp when this API client was registered.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Timestamp of the most recent API request from this client.</summary>
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// JSON array of permission strings defining what actions this client is authorized to perform.
        /// Example: ["licenses.read", "licenses.validate", "devices.read"]
        /// </summary>
        public string Permissions { get; set; } = "[]";
    }
}
