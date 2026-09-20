namespace LicensePlatform.Domain.Enums
{
    /// <summary>
    /// Defines the duration type of a software license.
    /// </summary>
    public enum LicenseType
    {
        /// <summary>Short-term trial license for evaluation purposes.</summary>
        Trial = 0,

        /// <summary>License valid for one day.</summary>
        Daily = 1,

        /// <summary>License valid for seven days.</summary>
        Weekly = 2,

        /// <summary>License valid for thirty days.</summary>
        Monthly = 3,

        /// <summary>License valid for ninety days.</summary>
        Quarterly = 4,

        /// <summary>License valid for three hundred sixty-five days.</summary>
        Yearly = 5,

        /// <summary>License with no expiration date.</summary>
        Lifetime = 6,

        /// <summary>License with a custom duration specified in days.</summary>
        Custom = 7
    }
}
