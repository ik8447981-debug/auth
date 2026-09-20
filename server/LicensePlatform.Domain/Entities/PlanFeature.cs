using System;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Join entity linking a license plan to its included features.
    /// </summary>
    public class PlanFeature
    {
        /// <summary>Unique identifier for the plan-feature association.</summary>
        public Guid Id { get; set; }

        /// <summary>Foreign key to the license plan.</summary>
        public Guid PlanId { get; set; }

        /// <summary>Foreign key to the product feature.</summary>
        public Guid FeatureId { get; set; }

        /// <summary>Navigation property to the license plan.</summary>
        public LicensePlan Plan { get; set; } = null!;

        /// <summary>Navigation property to the product feature.</summary>
        public ProductFeature Feature { get; set; } = null!;
    }
}
