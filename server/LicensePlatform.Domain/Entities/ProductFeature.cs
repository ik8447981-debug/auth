using System;
using System.Collections.Generic;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Represents a feature available within a specific product.
    /// </summary>
    public class ProductFeature
    {
        /// <summary>Unique identifier for the feature.</summary>
        public Guid FeatureId { get; set; }

        /// <summary>Foreign key to the parent product.</summary>
        public Guid ProductId { get; set; }

        /// <summary>Internal key used to reference the feature in code (e.g., ADVANCED_SCAN).</summary>
        public string FeatureKey { get; set; } = string.Empty;

        /// <summary>User-friendly display name of the feature.</summary>
        public string FeatureName { get; set; } = string.Empty;

        /// <summary>Detailed description of what the feature provides.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Whether this feature is currently enabled for use.</summary>
        public bool IsEnabled { get; set; }

        /// <summary>Timestamp when the feature was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Navigation property to the parent product.</summary>
        public Product Product { get; set; } = null!;

        /// <summary>Navigation property for plan features that include this feature.</summary>
        public ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();

        /// <summary>Navigation property for license features that reference this feature.</summary>
        public ICollection<LicenseFeature> LicenseFeatures { get; set; } = new List<LicenseFeature>();
    }
}
