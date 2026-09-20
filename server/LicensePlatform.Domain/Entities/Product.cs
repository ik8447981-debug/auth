using System;
using System.Collections.Generic;
using LicensePlatform.Domain.Enums;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Represents a software product managed by the licensing platform.
    /// </summary>
    public class Product
    {
        /// <summary>Unique identifier for the product.</summary>
        public Guid ProductId { get; set; }

        /// <summary>Internal product name used in API calls and licensing codes.</summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>User-friendly display name shown in the admin panel.</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Detailed description of the product and its capabilities.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Unique short code for the product (e.g., CLEANER, BACKUP).</summary>
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>Current lifecycle status of the product.</summary>
        public ProductStatus Status { get; set; }

        /// <summary>Current released version number of the product.</summary>
        public string CurrentVersion { get; set; } = string.Empty;

        /// <summary>Minimum version that is still supported for license validation.</summary>
        public string MinimumSupportedVersion { get; set; } = string.Empty;

        /// <summary>Latest version available for download.</summary>
        public string LatestVersion { get; set; } = string.Empty;

        /// <summary>Timestamp when the product was created in the system.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Timestamp when the product was last updated.</summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>Indicates whether this product has been soft-deleted.</summary>
        public bool IsDeleted { get; set; }

        /// <summary>Navigation property for all license plans associated with this product.</summary>
        public ICollection<LicensePlan> Plans { get; set; } = new List<LicensePlan>();

        /// <summary>Navigation property for all features available in this product.</summary>
        public ICollection<ProductFeature> Features { get; set; } = new List<ProductFeature>();

        /// <summary>Navigation property for all licenses issued for this product.</summary>
        public ICollection<License> Licenses { get; set; } = new List<License>();

        /// <summary>Navigation property for all customers associated with this product.</summary>
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
