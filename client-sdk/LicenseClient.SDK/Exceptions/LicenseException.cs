namespace LicenseClient.SDK.Exceptions;

/// <summary>
/// Base exception for all license-related errors.
/// Contains an error code for programmatic handling and a message for display.
/// </summary>
public class LicenseException : Exception
{
    /// <summary>
    /// Machine-readable error code for programmatic handling.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Creates a new LicenseException.
    /// </summary>
    /// <param name="errorCode">Machine-readable error code.</param>
    /// <param name="message">Human-readable error message.</param>
    public LicenseException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode ?? "UNKNOWN";
    }

    /// <summary>
    /// Creates a new LicenseException with an inner exception.
    /// </summary>
    /// <param name="errorCode">Machine-readable error code.</param>
    /// <param name="message">Human-readable error message.</param>
    /// <param name="innerException">The underlying exception.</param>
    public LicenseException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode ?? "UNKNOWN";
    }

    public override string ToString()
    {
        return $"[{ErrorCode}] {Message}";
    }
}

/// <summary>
/// Thrown when license activation fails.
/// </summary>
public class LicenseActivationException : LicenseException
{
    public LicenseActivationException(string message)
        : base("ACTIVATION_FAILED", message) { }

    public LicenseActivationException(string message, Exception innerException)
        : base("ACTIVATION_FAILED", message, innerException) { }
}

/// <summary>
/// Thrown when license validation fails.
/// </summary>
public class LicenseValidationException : LicenseException
{
    public LicenseValidationException(string message)
        : base("VALIDATION_FAILED", message) { }

    public LicenseValidationException(string message, Exception innerException)
        : base("VALIDATION_FAILED", message, innerException) { }
}

/// <summary>
/// Thrown when a license is used with the wrong product.
/// </summary>
public class ProductMismatchException : LicenseException
{
    public ProductMismatchException(string expected, string actual)
        : base("PRODUCT_MISMATCH",
            $"License is for product '{actual}', but this application requires '{expected}'.") { }
}

/// <summary>
/// Thrown when the device limit for a license has been reached.
/// </summary>
public class DeviceLimitException : LicenseException
{
    public DeviceLimitException(int maxDevices)
        : base("DEVICE_LIMIT",
            $"Device limit reached. This license allows a maximum of {maxDevices} device(s).") { }
}

/// <summary>
/// Thrown when the offline grace period has expired.
/// </summary>
public class OfflineGraceExpiredException : LicenseException
{
    public OfflineGraceExpiredException(int graceHours)
        : base("OFFLINE_GRACE_EXPIRED",
            $"Offline grace period ({graceHours}h) has expired. Please connect to the internet to revalidate.") { }
}

/// <summary>
/// Thrown when a clock rollback is detected.
/// </summary>
public class ClockRollbackException : LicenseException
{
    public ClockRollbackException(TimeSpan rollbackDuration)
        : base("CLOCK_ROLLBACK",
            $"Clock rollback detected ({rollbackDuration.TotalMinutes:F1} minutes). " +
            "Please ensure your system clock is correct.") { }
}

/// <summary>
/// Thrown when signature verification fails.
/// </summary>
public class SignatureVerificationException : LicenseException
{
    public SignatureVerificationException()
        : base("SIGNATURE_INVALID",
            "License signature verification failed. The license data may have been tampered with.") { }

    public SignatureVerificationException(string details)
        : base("SIGNATURE_INVALID",
            $"License signature verification failed: {details}") { }
}

/// <summary>
/// Thrown when the server is unreachable.
/// </summary>
public class ServerUnavailableException : LicenseException
{
    public ServerUnavailableException(string serverUrl)
        : base("SERVER_UNAVAILABLE",
            $"License server is unreachable: {serverUrl}. Using cached license state.") { }

    public ServerUnavailableException(string serverUrl, Exception innerException)
        : base("SERVER_UNAVAILABLE",
            $"License server is unreachable: {serverUrl}. Using cached license state.",
            innerException) { }
}

/// <summary>
/// Thrown when network connectivity is unavailable.
/// </summary>
public class NetworkUnavailableException : LicenseException
{
    public NetworkUnavailableException()
        : base("NETWORK_UNAVAILABLE",
            "No network connectivity available. Using cached license state.") { }

    public NetworkUnavailableException(Exception innerException)
        : base("NETWORK_UNAVAILABLE",
            "No network connectivity available. Using cached license state.",
            innerException) { }
}
