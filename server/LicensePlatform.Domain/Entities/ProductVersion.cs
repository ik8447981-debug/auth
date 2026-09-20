using System;

namespace LicensePlatform.Domain.Entities
{
    /// <summary>
    /// Represents a released version of a product.
    /// </summary>
    public class ProductVersion
    {
        /// <summary>Unique identifier for the version record.</summary>
        public Guid VersionId { get; set; }

        /// <summary>Foreign key to the parent product.</summary>
        public Guid ProductId { get; set; }

        /// <summary>The version number string (e.g., 1.0.0).</summary>
        public string VersionNumber { get; set; } = string.Empty;

        /// <summary>Timestamp when this version was released.</summary>
        public DateTime ReleasedAt { get; set; }

        /// <summary>Whether this is the latest version of the product.</summary>
        public bool IsLatest { get; set; }

        /// <summary>Timestamp when the version record was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Navigation property to the parent product.</summary>
        public Product Product { get; set; } = null!;
    }
}
