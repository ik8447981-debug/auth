# License Platform - Client SDK Documentation

## Overview

The LicenseClient.SDK is a C# class library for integrating license management into Windows EXE applications.

## Installation

Add a project reference to `LicenseClient.SDK.csproj` or install as NuGet package.

## Quick Start

```csharp
using LicenseClient.SDK.Core;
using LicenseClient.SDK.Extensions;

// Configure
var config = new LicenseConfiguration
{
    ServerUrl = "https://license.yourdomain.com",
    ProductId = "CLEANER",
    ProductCode = "CLNR",
    PublicKeyPem = "-----BEGIN PUBLIC KEY-----\n...",
    ApplicationVersion = "1.2.0"
};

// Register with DI
services.AddLicenseClient(config);

// Use in your application
var licenseClient = serviceProvider.GetRequiredService<LicenseClient>();
await licenseClient.InitializeAsync();
```

## API Reference

### LicenseClient

#### Initialize
```csharp
await licenseClient.InitializeAsync();
```
Validates configuration, loads cached state, verifies public key.

#### Activate
```csharp
var result = await licenseClient.ActivateAsync("CLNR-7K4P-X92M-Q8ZT");
if (result.Success)
{
    Console.WriteLine("License activated!");
    Console.WriteLine($"Expires: {result.ExpiresAt}");
    Console.WriteLine($"Features: {string.Join(", ", result.Features)}");
}
```

#### Validate
```csharp
var result = await licenseClient.ValidateAsync();
if (!result.IsValid)
{
    // Handle invalid license
    Console.WriteLine($"License invalid: {result.ErrorCode}");
}
```

#### Deactivate
```csharp
await licenseClient.DeactivateAsync();
```

#### GetStatus
```csharp
var status = licenseClient.GetStatus();
// LicenseStatus enum: Unknown, Active, Expired, Suspended, etc.
```

#### GetLicenseInfo
```csharp
var info = licenseClient.GetLicenseInfo();
Console.WriteLine($"Plan: {info.Plan}");
Console.WriteLine($"Expires: {info.ExpiryDate}");
Console.WriteLine($"Features: {string.Join(", ", info.Features)}");
```

#### HasFeature
```csharp
if (licenseClient.HasFeature("advanced_cleanup"))
{
    // Enable advanced cleanup feature
}
```

#### GetExpiry
```csharp
var expiry = licenseClient.GetExpiry();
if (expiry.HasValue && expiry.Value < DateTime.UtcNow.AddDays(7))
{
    Console.WriteLine("License expiring soon!");
}
```

#### GetPlan
```csharp
var plan = licenseClient.GetPlan();
// Returns: "Professional", "Basic", etc.
```

## Configuration Options

```csharp
public class LicenseConfiguration
{
    public string ServerUrl { get; set; }           // Required
    public string ProductId { get; set; }           // Required
    public string ProductCode { get; set; }         // Required
    public string PublicKeyPem { get; set; }        // Required
    public int ValidationIntervalMinutes { get; set; } = 60;
    public int OfflineGraceHours { get; set; } = 24;
    public string ApplicationVersion { get; set; } = "1.0.0";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
}
```

## Error Handling

```csharp
try
{
    await licenseClient.ActivateAsync("INVALID-KEY");
}
catch (LicenseActivationException ex)
{
    Console.WriteLine($"Activation failed: {ex.ErrorCode}");
    Console.WriteLine($"Message: {ex.Message}");
}
catch (ProductMismatchException)
{
    Console.WriteLine("This license is not for this product");
}
catch (DeviceLimitException)
{
    Console.WriteLine("Maximum devices reached");
}
catch (OfflineGraceExpiredException)
{
    Console.WriteLine("Please connect to internet to validate license");
}
```

## Offline Support

The SDK automatically handles offline scenarios:

1. **Cached License**: Stored locally with DPAPI protection
2. **Grace Period**: Configurable offline grace period
3. **Automatic Fallback**: Uses cached state when server unavailable
4. **Revalidation**: Automatically validates when connection restored

```csharp
// Offline grace is automatic
// No special code needed
var result = await licenseClient.ValidateAsync();
// Will use cached state if server unavailable, within grace period
```

## Feature Entitlement

```csharp
// Features are loaded from signed license response
// Never self-declared

var features = licenseClient.GetLicenseInfo().Features;

if (licenseClient.HasFeature("basic_cleanup"))
{
    EnableBasicCleanup();
}

if (licenseClient.HasFeature("advanced_cleanup"))
{
    EnableAdvancedCleanup();
}

if (licenseClient.HasFeature("automation"))
{
    EnableAutomation();
}
```

## Integration Example

```csharp
using LicenseClient.SDK.Core;

public partial class MainForm : Form
{
    private readonly LicenseClient _licenseClient;

    public MainForm(LicenseClient licenseClient)
    {
        _licenseClient = licenseClient;
        InitializeComponent();
    }

    private async void MainForm_Load(object sender, EventArgs e)
    {
        try
        {
            await _licenseClient.InitializeAsync();

            if (_licenseClient.GetStatus() == LicenseStatus.Active)
            {
                LoadMainApplication();
            }
            else
            {
                ShowActivationScreen();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"License error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

    private async void btnActivate_Click(object sender, EventArgs e)
    {
        var result = await _licenseClient.ActivateAsync(txtLicenseKey.Text);
        if (result.Success)
        {
            LoadMainApplication();
        }
        else
        {
            MessageBox.Show($"Activation failed: {result.ErrorMessage}");
        }
    }

    private void LoadMainApplication()
    {
        // Check features before enabling functionality
        if (_licenseClient.HasFeature("advanced_cleanup"))
        {
            btnAdvancedCleanup.Visible = true;
        }

        if (_licenseClient.HasFeature("automation"))
        {
            btnAutomation.Visible = true;
        }

        // Show license info in status bar
        var info = _licenseClient.GetLicenseInfo();
        statusLabel.Text = $"Plan: {info.Plan} | Expires: {info.ExpiryDate?.ToString("MMM dd, yyyy") ?? "Never"}";
    }
}
```

## Security Notes

### What's in the SDK
- Public verification key
- License activation/validation logic
- Local cache with DPAPI
- Signature verification

### What's NOT in the SDK
- Private signing key
- Database credentials
- Admin secrets
- Server-side business logic

### Trust Model
- Server is the authority
- Client verifies server signatures
- Client does NOT make trust decisions
- All validation is server-side

## Thread Safety

The LicenseClient is thread-safe for concurrent access. Multiple threads can call Validate() simultaneously.

## Disposal

```csharp
// LicenseClient implements IDisposable
using var licenseClient = new LicenseClient(config);
await licenseClient.InitializeAsync();
// ... use license client
// Automatically disposed
```
