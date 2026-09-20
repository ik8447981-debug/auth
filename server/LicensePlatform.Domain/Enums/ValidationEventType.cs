namespace LicensePlatform.Domain.Enums
{
    /// <summary>
    /// Defines the types of validation events that can occur during license verification.
    /// </summary>
    public enum ValidationEventType
    {
        /// <summary>Initial license activation request from a client device.</summary>
        Activation = 0,

        /// <summary>Periodic license validation check.</summary>
        Validation = 1,

        /// <summary>Heartbeat signal from a client to confirm continued operation.</summary>
        Heartbeat = 2,

        /// <summary>Client-initiated license deactivation request.</summary>
        Deactivation = 3
    }
}
