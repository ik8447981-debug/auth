using System;
using System.Collections.Generic;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Represents a customer who holds software licenses.
    /// </summary>
    public class Customer
    {
        /// <summary>Unique identifier for the customer.</summary>
        public Guid CustomerId { get; set; }

        /// <summary>Foreign key to the associated product.</summary>
        public Guid ProductId { get; set; }

        /// <summary>Full name of the customer.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Email address of the customer (unique per product).</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Additional notes about the customer.</summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>Whether this customer account is currently active.</summary>
        public bool IsActive { get; set; }

        /// <summary>Timestamp when the customer was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Timestamp when the customer was last updated.</summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>Navigation property to the associated product.</summary>
        public Product Product { get; set; } = null!;

        /// <summary>Navigation property for all licenses held by this customer.</summary>
        public ICollection<License> Licenses { get; set; } = new List<License>();
    }
}
