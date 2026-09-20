namespace LicensePlatform.Domain.Enums
{
    /// <summary>
    /// Represents the lifecycle status of a product in the platform.
    /// </summary>
    public enum ProductStatus
    {
        /// <summary>Product is fully operational and available for licensing.</summary>
        Active = 0,

        /// <summary>Product is temporarily unavailable for new licenses.</summary>
        Disabled = 1,

        /// <summary>Product has been archived and is no longer maintained.</summary>
        Archived = 2,

        /// <summary>Product is undergoing scheduled maintenance.</summary>
        Maintenance = 3
    }
}
