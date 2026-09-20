using System;
using System.Collections.Generic;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Represents a licensing plan that defines terms and features for a product.
    /// </summary>
    public class LicensePlan
    {
        /// <summary>Unique identifier for the plan.</summary>
        public Guid PlanId { get; set; }

        /// <summary>Foreign key to the parent product.</summary>
        public Guid ProductId { get; set; }

        /// <summary>User-friendly name of the plan (e.g., "Basic", "Pro", "Enterprise").</summary>
        public string PlanName { get; set; } = string.Empty;

        /// <summary>Duration of the plan in days. Use -1 for lifetime plans.</summary>
        public int DurationDays { get; set; }

        /// <summary>Maximum number of devices allowed under this plan.</summary>
        public int MaxDevices { get; set; }

        /// <summary>Number of hours a license can work offline before requiring re-validation.</summary>
        public int OfflineGraceHours { get; set; }

        /// <summary>How often the client should check in with the server (in minutes).</summary>
        public int ValidationIntervalMinutes { get; set; }

        /// <summary>Whether this plan is currently active and available for new licenses.</summary>
        public bool IsActive { get; set; }

        /// <summary>Timestamp when the plan was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Timestamp when the plan was last updated.</summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>Navigation property to the parent product.</summary>
        public Product Product { get; set; } = null!;

        /// <summary>Navigation property for plan-feature associations.</summary>
        public ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();

        /// <summary>Navigation property for licenses created under this plan.</summary>
        public ICollection<License> Licenses { get; set; } = new List<License>();
    }
}
