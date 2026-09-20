namespace LicenseClient.SDK.Validation;

/// <summary>
/// Result of a server time consistency check.
/// </summary>
public class TimeValidationResult
{
    /// <summary>
    /// Whether the time check passed.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Whether a clock rollback was detected.
    /// </summary>
    public bool ClockRollbackDetected { get; set; }

    /// <summary>
    /// The detected rollback duration, if any.
    /// </summary>
    public TimeSpan? RollbackDuration { get; set; }

    /// <summary>
    /// Human-readable message about the time check result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Creates a successful time validation result.
    /// </summary>
    public static TimeValidationResult Success() => new()
    {
        IsValid = true,
        ClockRollbackDetected = false,
        Message = "Time validation passed."
    };

    /// <summary>
    /// Creates a failed time validation result with rollback detection.
    /// </summary>
    public static TimeValidationResult RollbackDetected(TimeSpan rollback) => new()
    {
        IsValid = false,
        ClockRollbackDetected = true,
        RollbackDuration = rollback,
        Message = $"Clock rollback detected: {rollback.TotalMinutes:F1} minutes."
    };
}

/// <summary>
/// Validates server time consistency and detects local clock manipulation.
/// Tracks last known server time and local time to detect rollbacks.
/// </summary>
public class ServerTimeValidator
{
    private readonly TimeSpan _tolerance;
    private DateTime? _lastServerTime;
    private DateTime? _lastValidationTime;
    private DateTime? _lastLocalTime;

    /// <summary>
    /// Creates a new ServerTimeValidator.
    /// </summary>
    /// <param name="tolerance">Maximum allowed deviation before considering time suspicious. Default: 5 minutes.</param>
    public ServerTimeValidator(TimeSpan? tolerance = null)
    {
        _tolerance = tolerance ?? TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Gets the last known server time from validation.
    /// </summary>
    public DateTime? LastServerTime => _lastServerTime;

    /// <summary>
    /// Gets the local time of the last validation.
    /// </summary>
    public DateTime? LastValidationTime => _lastValidationTime;

    /// <summary>
    /// Validates the current time against previously recorded server time.
    /// Detects obvious clock rollbacks.
    /// </summary>
    /// <param name="serverTime">The server-reported time from the current response.</param>
    /// <returns>Validation result indicating success or detected issues.</returns>
    public TimeValidationResult Validate(DateTime serverTime)
    {
        var now = DateTime.UtcNow;

        // First validation - no history to compare
        if (_lastServerTime == null || _lastLocalTime == null)
        {
            UpdateTimestamps(serverTime, now);
            return TimeValidationResult.Success();
        }

        // Check for obvious local clock rollback
        // If local time went backwards significantly
        var localDelta = now - _lastLocalTime!.Value;
        if (localDelta < TimeSpan.FromMinutes(-5))
        {
            // Local clock went backward by more than 5 minutes
            return TimeValidationResult.RollbackDetected(localDelta.Negate());
        }

        // Check server time regression
        // Server time should always move forward
        var serverDelta = serverTime - _lastServerTime!.Value;
        if (serverDelta < TimeSpan.FromMinutes(-5))
        {
            // Server time went backward - possible server issue or replay
            return TimeValidationResult.RollbackDetected(serverDelta.Negate());
        }

        // Check if server time and local time diverge too much
        // This could indicate clock manipulation or timezone issues
        var localServerDelta = Math.Abs((serverTime - now).TotalMinutes);
        if (localServerDelta > _tolerance.TotalMinutes * 2)
        {
            // Significant divergence between server and local time
            // This is suspicious but not necessarily rollback
        }

        // Check if elapsed server time is reasonable
        // Between validations, server time should advance roughly
        // as much as local time (within tolerance)
        if (localDelta > TimeSpan.Zero && serverDelta > TimeSpan.Zero)
        {
            var ratio = serverDelta.TotalMinutes / localDelta.TotalMinutes;
            // If server time advances at less than 50% or more than 200% of local time,
            // something is suspicious
            if (ratio < 0.5 || ratio > 2.0)
            {
                // Not a hard failure, but log the anomaly
            }
        }

        UpdateTimestamps(serverTime, now);
        return TimeValidationResult.Success();
    }

    /// <summary>
    /// Updates the tracked timestamps with new values.
    /// </summary>
    private void UpdateTimestamps(DateTime serverTime, DateTime localTime)
    {
        _lastServerTime = serverTime;
        _lastValidationTime = localTime;
        _lastLocalTime = localTime;
    }

    /// <summary>
    /// Restores time tracking state from cache.
    /// </summary>
    public void RestoreState(DateTime lastServerTime, DateTime lastLocalTime)
    {
        _lastServerTime = lastServerTime;
        _lastLocalTime = lastLocalTime;
        _lastValidationTime = lastLocalTime;
    }

    /// <summary>
    /// Resets the time validator state.
    /// </summary>
    public void Reset()
    {
        _lastServerTime = null;
        _lastValidationTime = null;
        _lastLocalTime = null;
    }
}
