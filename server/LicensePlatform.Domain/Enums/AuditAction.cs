namespace LicensePlatform.Domain.Enums
{
    /// <summary>
    /// Defines the types of actions that can be recorded in the audit log.
    /// </summary>
    public enum AuditAction
    {
        /// <summary>Administrator successfully logged in.</summary>
        AdminLogin = 0,

        /// <summary>Administrator login attempt failed.</summary>
        AdminLoginFailed = 1,

        /// <summary>A new product was created.</summary>
        ProductCreated = 2,

        /// <summary>An existing product was updated.</summary>
        ProductUpdated = 3,

        /// <summary>A product was disabled.</summary>
        ProductDisabled = 4,

        /// <summary>A new license was created.</summary>
        LicenseCreated = 5,

        /// <summary>A license was activated by a client.</summary>
        LicenseActivated = 6,

        /// <summary>A license duration was extended.</summary>
        LicenseExtended = 7,

        /// <summary>A license was suspended by an administrator.</summary>
        LicenseSuspended = 8,

        /// <summary>A license was permanently revoked.</summary>
        LicenseRevoked = 9,

        /// <summary>A device was added to a license.</summary>
        DeviceAdded = 10,

        /// <summary>A device was reset and re-registered.</summary>
        DeviceReset = 11,

        /// <summary>A device was removed from a license.</summary>
        DeviceRemoved = 12,

        /// <summary>A new customer account was created.</summary>
        CustomerCreated = 13,

        /// <summary>System settings were changed.</summary>
        SettingsChanged = 14
    }
}
