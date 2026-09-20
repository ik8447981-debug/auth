using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LicenseClient.SDK.Caching;

/// <summary>
/// Local license state persistence using Windows Data Protection API (DPAPI).
/// Provides encrypted storage of license state on the local machine.
/// Cache location: %APPDATA%\{ProductId}\license.cache
/// </summary>
public class LicenseCache : ILicenseCache
{
    private readonly string _cacheDirectory;
    private readonly string _cacheFilePath;
    private readonly string _productId;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Creates a new LicenseCache instance.
    /// </summary>
    /// <param name="productId">Product identifier for cache path isolation.</param>
    /// <param name="cacheDirectory">Optional custom cache directory.</param>
    public LicenseCache(string productId, string? cacheDirectory = null)
    {
        _productId = productId ?? throw new ArgumentNullException(nameof(productId));

        _cacheDirectory = cacheDirectory
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                productId);

        _cacheFilePath = Path.Combine(_cacheDirectory, "license.cache");
    }

    /// <summary>
    /// Loads the cached license state. Returns null if no cache exists
    /// or if the cache is corrupted/unreadable.
    /// </summary>
    public CachedLicenseState? Load()
    {
        try
        {
            if (!File.Exists(_cacheFilePath))
                return null;

            // Read the encrypted bytes from disk
            var encryptedBytes = File.ReadAllBytes(_cacheFilePath);
            if (encryptedBytes.Length == 0)
                return null;

            // Decrypt using DPAPI
            var decryptedBytes = ProtectedData.Unprotect(
                encryptedBytes,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser);

            // Deserialize JSON
            var json = Encoding.UTF8.GetString(decryptedBytes);
            var state = JsonSerializer.Deserialize<CachedLicenseState>(json, JsonOptions);

            return state;
        }
        catch (CryptographicException)
        {
            // DPAPI decryption failed (e.g., different user context)
            return null;
        }
        catch (JsonException)
        {
            // Corrupted cache data
            return null;
        }
        catch (IOException)
        {
            // File system error
            return null;
        }
    }

    /// <summary>
    /// Saves the license state to disk with DPAPI encryption.
    /// Creates the cache directory if it doesn't exist.
    /// </summary>
    public void Save(CachedLicenseState state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));

        try
        {
            // Ensure cache directory exists
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }

            // Serialize to JSON
            var json = JsonSerializer.Serialize(state, JsonOptions);
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            // Encrypt with DPAPI (CurrentUser scope)
            var encryptedBytes = ProtectedData.Protect(
                jsonBytes,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser);

            // Write atomically: write to temp, then move
            var tempPath = _cacheFilePath + ".tmp";
            File.WriteAllBytes(tempPath, encryptedBytes);

            // Replace existing file
            if (File.Exists(_cacheFilePath))
            {
                File.Delete(_cacheFilePath);
            }
            File.Move(tempPath, _cacheFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Clean up temp file if it exists
            try
            {
                var tempPath = _cacheFilePath + ".tmp";
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
            catch { /* Best effort cleanup */ }

            // Re-throw as a license exception for consistent error handling
            throw new Exceptions.LicenseException(
                "CACHE_SAVE_FAILED",
                $"Failed to save license cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears the cached license state by deleting the cache file.
    /// </summary>
    public void Clear()
    {
        try
        {
            if (File.Exists(_cacheFilePath))
            {
                // Overwrite with zeros before deleting (defense in depth)
                var fileInfo = new FileInfo(_cacheFilePath);
                if (fileInfo.Length > 0)
                {
                    var zeroBytes = new byte[fileInfo.Length];
                    File.WriteAllBytes(_cacheFilePath, zeroBytes);
                }
                File.Delete(_cacheFilePath);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }

    /// <summary>
    /// Checks whether a valid cache file exists.
    /// </summary>
    public bool IsValid()
    {
        return File.Exists(_cacheFilePath)
            && new FileInfo(_cacheFilePath).Length > 0;
    }
}
