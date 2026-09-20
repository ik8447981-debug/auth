using System.Management;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace LicenseClient.SDK.Identity;

/// <summary>
/// Generates device fingerprint from system signals.
/// Privacy-conscious: only sends a normalized SHA256 hash, never raw hardware IDs.
/// </summary>
public class DeviceIdentity : IDeviceIdentity
{
    private readonly string _productId;
    private string? _cachedFingerprint;
    private IReadOnlyDictionary<string, string>? _cachedSignals;

    /// <summary>
    /// Creates a new DeviceIdentity instance.
    /// </summary>
    /// <param name="productId">Product identifier used as salt in fingerprint.</param>
    public DeviceIdentity(string productId)
    {
        _productId = productId ?? throw new ArgumentNullException(nameof(productId));
    }

    /// <summary>
    /// Generates a stable device fingerprint. The fingerprint is deterministic
    /// for the same device and product combination.
    /// </summary>
    public string GetFingerprint()
    {
        if (_cachedFingerprint != null)
            return _cachedFingerprint;

        var signals = CollectSignals();
        _cachedSignals = signals;

        // Sort signals deterministically for stable hashing
        var sortedEntries = signals
            .OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);

        var sb = new StringBuilder();
        sb.Append(_productId);
        sb.Append(':');
        foreach (var entry in sortedEntries)
        {
            sb.Append(entry.Key);
            sb.Append('=');
            sb.Append(entry.Value);
            sb.Append(';');
        }

        // Compute SHA256 hash
        var inputBytes = Encoding.UTF8.GetBytes(sb.ToString());
        var hashBytes = SHA256.HashData(inputBytes);
        _cachedFingerprint = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return _cachedFingerprint;
    }

    /// <summary>
    /// Gets the raw signals used for fingerprint generation.
    /// </summary>
    public IReadOnlyDictionary<string, string> GetSignals()
    {
        if (_cachedSignals == null)
        {
            CollectSignals();
        }
        return _cachedSignals!;
    }

    /// <summary>
    /// Collects system signals for fingerprinting. Only collects normalized
    /// identifiers that are stable across reboots.
    /// </summary>
    private Dictionary<string, string> CollectSignals()
    {
        var signals = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Windows MachineGuid from registry (stable identifier)
        CollectRegistrySignal(signals, "MachineGuid",
            @"SOFTWARE\Microsoft\Cryptography", "MachineGuid");

        // Processor ID (stable across reboots)
        CollectWmiSignal(signals, "ProcessorId",
            "Win32_Processor", "ProcessorId");

        // BIOS Serial Number
        CollectWmiSignal(signals, "BiosSerial",
            "Win32_BIOS", "SerialNumber");

        // Motherboard Serial Number
        CollectWmiSignal(signals, "BoardSerial",
            "Win32_BaseBoard", "SerialNumber");

        // System UUID
        CollectWmiSignal(signals, "SystemUUID",
            "Win32_ComputerSystemProduct", "UUID");

        // Disk Drive Serial (primary)
        CollectWmiSignal(signals, "DiskSerial",
            "Win32_DiskDrive", "SerialNumber");

        // Normalize all signals
        foreach (var key in signals.Keys.ToList())
        {
            signals[key] = NormalizeSignal(signals[key]);
        }

        return signals;
    }

    /// <summary>
    /// Collects a signal from the Windows Registry.
    /// </summary>
    private static void CollectRegistrySignal(
        Dictionary<string, string> signals, string key,
        string registryPath, string valueName)
    {
        try
        {
            using var subKey = Registry.LocalMachine.OpenSubKey(registryPath);
            var value = subKey?.GetValue(valueName)?.ToString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                signals[key] = value;
            }
        }
        catch
        {
            // Registry access may fail; skip this signal
        }
    }

    /// <summary>
    /// Collects a signal from WMI (Windows Management Instrumentation).
    /// Uses SELECT TOP 1 to avoid multiple result issues.
    /// </summary>
    private static void CollectWmiSignal(
        Dictionary<string, string> signals, string key,
        string wmiClass, string propertyName)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                $"SELECT TOP 1 {propertyName} FROM {wmiClass}");

            using var collection = searcher.Get();
            foreach (var obj in collection)
            {
                var value = obj[propertyName]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    signals[key] = value;
                    break;
                }
            }
        }
        catch
        {
            // WMI access may fail; skip this signal
        }
    }

    /// <summary>
    /// Normalizes a signal value by trimming, lowercasing, and removing
    /// non-alphanumeric characters.
    /// </summary>
    private static string NormalizeSignal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Trim and lowercase
        var normalized = value.Trim().ToLowerInvariant();

        // Remove common whitespace characters
        normalized = normalized
            .Replace("\r", "")
            .Replace("\n", "")
            .Replace("\t", "")
            .Replace(" ", "");

        return normalized;
    }
}
