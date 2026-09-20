using System;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Junction entity that associates a specific feature with an individual license,
    /// allowing per-license feature overrides.
    /// </summary>
    public class LicenseFeature
    {
        /// <summary>Unique identifier for this license-feature association.</summary>
        public Guid LicenseFeatureId { get; set; }

        /// <summary>Foreign key reference to the license.</summary>
        public Guid LicenseId { get; set; }

        /// <summary>Foreign key reference to the product feature.</summary>
        public Guid FeatureId { get; set; }

        /// <summary>Navigation property to the license.</summary>
        public License License { get; set; } = null!;

        /// <summary>Navigation property to the product feature.</summary>
        public ProductFeature Feature { get; set; } = null!;
    }
}
