using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Represents a cryptographic signing key pair used to sign and verify license keys.
    /// Supports key rotation through versioning.
    /// </summary>
    public class SigningKey
    {
        /// <summary>Unique identifier for this signing key record.</summary>
        public Guid SigningKeyId { get; set; }

        /// <summary>Version number of this key (incremented on each key rotation).</summary>
        public int KeyVersion { get; set; }

        /// <summary>PEM-encoded public key used for offline license verification.</summary>
        [Required]
        public string PublicKeyPem { get; set; } = string.Empty;

        /// <summary>Indicates whether this key is currently active for signing new licenses.</summary>
        public bool IsActive { get; set; }

        /// <summary>Timestamp when this key record was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Timestamp when this key was activated for use (null if not yet activated).</summary>
        public DateTime? ActivatedAt { get; set; }

        /// <summary>Timestamp when this key was deactivated (null if still active).</summary>
        public DateTime? DeactivatedAt { get; set; }

        /// <summary>Navigation property for validation events that used this signing key.</summary>
        public ICollection<ValidationEvent> Validations { get; set; } = new List<ValidationEvent>();
    }
}
