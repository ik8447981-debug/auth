namespace LicensePlatform.Domain.Enums
{
    /// <summary>
    /// Defines the role and permission level of an administrative user.
    /// </summary>
    public enum AdminRole
    {
        /// <summary>Full system access with all administrative privileges.</summary>
        SuperAdmin = 0,

        /// <summary>Standard administrative access for managing products and licenses.</summary>
        Admin = 1,

        /// <summary>Support-level access for customer assistance and basic troubleshooting.</summary>
        Support = 2,

        /// <summary>Read-only access to view data and reports.</summary>
        Viewer = 3
    }
}
