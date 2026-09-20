using System;
using System.Collections.Generic;
using LicensePlatform.Domain.Enums;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Represents a software license issued to a customer.
    /// </summary>
    public class License
    {
        /// <summary>Unique identifier for the license.</summary>
        public Guid LicenseId { get; set; }

        /// <summary>The unique license key string (e.g., LP-PRODUCTCODE-XXXX-XXXX-XXXX).</summary>
        public string LicenseKey { get; set; } = string.Empty;

        /// <summary>Foreign key to the product this license is for.</summary>
        public Guid ProductId { get; set; }

        /// <summary>Foreign key to the customer this license belongs to.</summary>
        public Guid? CustomerId { get; set; }

        /// <summary>Foreign key to the license plan this license follows.</summary>
        public Guid PlanId { get; set; }

        /// <summary>The type of license (Trial, Monthly, Yearly, Lifetime, etc.).</summary>
        public LicenseType LicenseType { get; set; }

        /// <summary>Current status of the license.</summary>
        public LicenseStatus Status { get; set; }

        /// <summary>When the license period begins.</summary>
        public DateTime StartDate { get; set; }

        /// <summary>When the license expires. Null for lifetime licenses.</summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>Maximum number of devices allowed to activate this license.</summary>
        public int MaxDevices { get; set; }

        /// <summary>Timestamp when the license was first activated.</summary>
        public DateTime? ActivatedAt { get; set; }

        /// <summary>Timestamp when the license was last validated.</summary>
        public DateTime? LastValidatedAt { get; set; }

        /// <summary>Number of consecutive failed validation attempts.</summary>
        public int FailedValidationCount { get; set; }

        /// <summary>Timestamp when the license was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Timestamp when the license was last updated.</summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>Navigation property to the product.</summary>
        public Product Product { get; set; } = null!;

        /// <summary>Navigation property to the customer.</summary>
        public Customer? Customer { get; set; }

        /// <summary>Navigation property to the license plan.</summary>
        public LicensePlan Plan { get; set; } = null!;

        /// <summary>Navigation property for devices registered to this license.</summary>
        public ICollection<LicenseDevice> Devices { get; set; } = new List<LicenseDevice>();

        /// <summary>Navigation property for features included in this license.</summary>
        public ICollection<LicenseFeature> LicenseFeatures { get; set; } = new List<LicenseFeature>();

        /// <summary>Navigation property for validation events for this license.</summary>
        public ICollection<ValidationEvent> ValidationEvents { get; set; } = new List<ValidationEvent>();
    }
}
