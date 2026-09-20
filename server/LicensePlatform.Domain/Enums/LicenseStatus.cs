namespace LicensePlatform.Domain.Enums
{
    /// <summary>
    /// Represents the current status of a software license.
    /// </summary>
    public enum LicenseStatus
    {
        /// <summary>License has been created but not yet activated.</summary>
        Created = 0,

        /// <summary>License is active and valid.</summary>
        Active = 1,

        /// <summary>License has expired due to time limits.</summary>
        Expired = 2,

        /// <summary>License has been temporarily suspended by an administrator.</summary>
        Suspended = 3,

        /// <summary>License has been permanently revoked.</summary>
        Revoked = 4,

        /// <summary>License has been administratively disabled.</summary>
        Disabled = 5
    }
}
