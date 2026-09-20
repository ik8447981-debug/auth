# License Platform - Product Integration Guide

## Overview

This guide explains how to integrate the License Client SDK into your Windows EXE application.

## Prerequisites

- .NET 8 SDK
- License Platform API running
- Product registered in admin panel
- Public signing key from admin panel

## Step 1: Add SDK Reference

```xml
<!-- In your .csproj file -->
<ItemGroup>
    <ProjectReference Include="..\..\client-sdk\LicenseClient.SDK\LicenseClient.SDK.csproj" />
</ItemGroup>
```

## Step 2: Configure the SDK

```csharp
// In your application startup
using LicenseClient.SDK.Core;
using LicenseClient.SDK.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure license client
var licenseConfig = new LicenseConfiguration
{
    ServerUrl = "https://license.yourdomain.com",
    ProductId = "YOUR_PRODUCT_ID",        // e.g., "CLEANER"
    ProductCode = "YOUR_PRODUCT_CODE",    // e.g., "CLNR"
    PublicKeyPem = LoadPublicKey(),       // From admin panel
    ApplicationVersion = "1.0.0",
    ValidationIntervalMinutes = 60,
    OfflineGraceHours = 24
};

builder.Services.AddLicenseClient(licenseConfig);
```

## Step 3: Initialize on Startup

```csharp
// In your main form/application
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
            CheckLicenseStatus();
        }
        catch (Exception ex)
        {
            ShowError($"License initialization failed: {ex.Message}");
        }
    }

    private void CheckLicenseStatus()
    {
        var status = _licenseClient.GetStatus();

        switch (status)
        {
            case LicenseStatus.Active:
                LoadMainApplication();
                break;

            case LicenseStatus.Expired:
                ShowLicenseExpired();
                break;

            case LicenseStatus.Suspended:
                ShowLicenseSuspended();
                break;

            case LicenseStatus.Revoked:
                ShowLicenseRevoked();
                break;

            case LicenseStatus.OfflineGrace:
                ShowOfflineWarning();
                LoadMainApplication();
                break;

            default:
                ShowActivationScreen();
                break;
        }
    }
}
```

## Step 4: Activation Screen

```csharp
private void ShowActivationScreen()
{
    var activationForm = new ActivationForm(_licenseClient);
    activationForm.ShowDialog();

    if (activationForm.Activated)
    {
        CheckLicenseStatus();
    }
    else
    {
        Application.Exit();
    }
}
```

### Activation Form

```csharp
public partial class ActivationForm : Form
{
    private readonly LicenseClient _licenseClient;
    public bool Activated { get; private set; }

    public ActivationForm(LicenseClient licenseClient)
    {
        _licenseClient = licenseClient;
        InitializeComponent();
    }

    private async void btnActivate_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtLicenseKey.Text))
        {
            ShowError("Please enter a license key");
            return;
        }

        btnActivate.Enabled = false;
        lblStatus.Text = "Activating...";

        try
        {
            var result = await _licenseClient.ActivateAsync(txtLicenseKey.Text.Trim());

            if (result.Success)
            {
                Activated = true;
                MessageBox.Show("License activated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            else
            {
                ShowError(result.ErrorMessage);
            }
        }
        catch (LicenseException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowError($"Activation failed: {ex.Message}");
        }
        finally
        {
            btnActivate.Enabled = true;
        }
    }
}
```

## Step 5: Feature-Gated UI

```csharp
private void LoadMainApplication()
{
    // Load features from license
    var features = _licenseClient.GetLicenseInfo().Features;

    // Show/hide UI elements based on features
    btnBasicCleanup.Visible = _licenseClient.HasFeature("basic_cleanup");
    btnAdvancedCleanup.Visible = _licenseClient.HasFeature("advanced_cleanup");
    btnAutomation.Visible = _licenseClient.HasFeature("automation");
    btnPremiumFeatures.Visible = _licenseClient.HasFeature("premium");

    // Show license info
    var info = _licenseClient.GetLicenseInfo();
    statusStrip.Items.Add($"Plan: {info.Plan}");
    statusStrip.Items.Add($"Expires: {info.ExpiryDate?.ToString("MMM dd, yyyy") ?? "Never"}");
}
```

## Step 6: Feature-Gated Logic

```csharp
private void RunCleanup()
{
    if (!_licenseClient.HasFeature("basic_cleanup"))
    {
        MessageBox.Show("Basic cleanup requires a license upgrade");
        return;
    }

    // Run basic cleanup
    DoBasicCleanup();

    // Check for advanced features
    if (_licenseClient.HasFeature("advanced_cleanup"))
    {
        DoAdvancedCleanup();
    }
}
```

## Step 7: Periodic Validation

The SDK automatically validates periodically. You can also trigger manual validation:

```csharp
private async void btnRefreshLicense_Click(object sender, EventArgs e)
{
    try
    {
        var result = await _licenseClient.ValidateAsync();

        if (result.IsValid)
        {
            MessageBox.Show("License is valid");
        }
        else
        {
            MessageBox.Show($"License invalid: {result.ErrorCode}");
            ShowLicenseExpired();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Validation error: {ex.Message}");
    }
}
```

## Step 8: Deactivation

```csharp
private async void btnDeactivate_Click(object sender, EventArgs e)
{
    var confirm = MessageBox.Show(
        "Are you sure you want to deactivate this license?",
        "Confirm Deactivation",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning);

    if (confirm == DialogResult.Yes)
    {
        try
        {
            await _licenseClient.DeactivateAsync();
            MessageBox.Show("License deactivated");
            ShowActivationScreen();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Deactivation failed: {ex.Message}");
        }
    }
}
```

## Complete Example

```csharp
using LicenseClient.SDK.Core;
using LicenseClient.SDK.Models;
using LicenseClient.SDK.Exceptions;

namespace MyApp;

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
        await _licenseClient.InitializeAsync();

        var status = _licenseClient.GetStatus();

        if (status == LicenseStatus.Active)
        {
            InitializeApplication();
        }
        else if (status == LicenseStatus.OfflineGrace)
        {
            MessageBox.Show(
                "Running in offline mode. License will be validated when connection is restored.",
                "Offline Mode",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            InitializeApplication();
        }
        else
        {
            ShowActivationScreen();
        }
    }

    private void InitializeApplication()
    {
        var info = _licenseClient.GetLicenseInfo();

        // Configure UI based on features
        foreach (var feature in info.Features)
        {
            switch (feature)
            {
                case "basic_cleanup":
                    btnBasicCleanup.Enabled = true;
                    break;
                case "advanced_cleanup":
                    btnAdvancedCleanup.Enabled = true;
                    break;
                case "automation":
                    btnAutomation.Enabled = true;
                    btnSchedule.Enabled = true;
                    break;
                case "premium":
                    btnPremiumFeatures.Enabled = true;
                    break;
            }
        }

        // Update status bar
        lblPlan.Text = $"Plan: {info.Plan}";
        lblExpiry.Text = info.ExpiryDate.HasValue
            ? $"Expires: {info.ExpiryDate:MMM dd, yyyy}"
            : "Lifetime License";
    }

    private async void btnBasicCleanup_Click(object sender, EventArgs e)
    {
        try
        {
            // Validate before operation
            var validation = await _licenseClient.ValidateAsync();
            if (!validation.IsValid)
            {
                MessageBox.Show("License validation failed. Please check your connection.");
                return;
            }

            // Run the feature
            await RunBasicCleanup();
        }
        catch (LicenseValidationException ex)
        {
            MessageBox.Show($"License error: {ex.Message}");
        }
    }
}
```

## Troubleshooting

### "Product Mismatch" Error
- Wrong ProductId configured
- Wrong public key
- License key is for different product

### "Device Limit Reached"
- Too many devices activated
- Reset devices via admin panel
- Or upgrade plan

### "License Expired"
- License has expired
- Extend via admin panel
- Or generate new license

### "Server Unavailable"
- Check internet connection
- SDK will use offline grace if configured
- Check server URL configuration

### "Invalid Signature"
- Public key doesn't match server
- Response was tampered with
- Contact support

### Activation Fails Silently
- Check network connectivity
- Verify server URL is correct
- Check firewall settings
- Review server logs
