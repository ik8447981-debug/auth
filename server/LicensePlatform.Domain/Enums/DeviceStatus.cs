namespace LicensePlatform.Domain.Enums
{
    /// <summary>
    /// Represents the status of a registered device.
    /// </summary>
    public enum DeviceStatus
    {
        /// <summary>Device is active and authorized to use the license.</summary>
        Active = 0,

        /// <summary>Device access has been temporarily suspended.</summary>
        Suspended = 1,

        /// <summary>Device has been removed from the license.</summary>
        Removed = 2
    }
}
