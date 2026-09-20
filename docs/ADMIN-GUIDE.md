# License Platform - Admin Guide

## Getting Started

### Login
1. Navigate to the admin panel URL
2. Enter your username and password
3. Click "Login"

**Default Admin Account:**
- Username: `admin`
- Password: `admin123` (change immediately in production)

## Dashboard

The main dashboard shows:
- **Total Products**: Number of registered products
- **Total Licenses**: All licenses across products
- **Active Licenses**: Currently valid licenses
- **Expired Licenses**: Past expiry date
- **Revoked Licenses**: Permanently invalidated
- **Active Devices**: Devices with active bindings
- **Activations Today**: New activations in last 24 hours
- **Validations Today**: Validation requests in last 24 hours

Charts show trends for activations, validations, and license distribution.

## Product Management

### Creating a Product

1. Click "Products" in the sidebar
2. Click "Create Product"
3. Fill in:
   - **Product Name**: Internal name (e.g., "Cleaner")
   - **Display Name**: Shown to users (e.g., "Cleaner Pro")
   - **Description**: Brief description
   - **Product Code**: Unique code (e.g., "CLNR") - used in license keys
4. Click "Create"

### Product Status

| Status | Description |
|--------|-------------|
| Active | Normal operation, new activations allowed |
| Disabled | No new activations, existing customers may continue |
| Archived | Hidden from lists, no operations |
| Maintenance | Temporary maintenance mode |

### Managing Product Features

1. Open Product Details
2. Click "Features" tab
3. Click "Add Feature"
4. Enter:
   - **Feature Key**: Programmatic name (e.g., "advanced_cleanup")
   - **Feature Name**: Display name (e.g., "Advanced Cleanup")
   - **Description**: What this feature provides
5. Toggle enabled/disabled with the switch

### Managing Product Versions

1. Open Product Details
2. Click "Versions" tab
3. Add versions with:
   - **Version Number**: Semantic version (e.g., "1.2.0")
   - **Blocked**: Whether this version is blocked
   - **ReleasedAt**: Release date
   - **Notes**: Version notes

## License Plan Management

### Creating a Plan

1. Open Product Details
2. Click "Plans" tab
3. Click "Create Plan"
4. Configure:
   - **Plan Name**: (e.g., "Professional")
   - **Duration**: Days (-1 for lifetime)
   - **Max Devices**: How many devices allowed
   - **Offline Grace Hours**: Hours allowed offline
   - **Validation Interval Minutes**: How often to check
   - **Features**: Select which features are included
5. Click "Create"

### Plan Examples

| Plan | Duration | Devices | Features |
|------|----------|---------|----------|
| Free | 30 days | 1 | Basic |
| Basic | 90 days | 1 | Basic, Cleanup |
| Pro | 365 days | 3 | Basic, Cleanup, Advanced |
| Premium | Lifetime | 5 | All features |

## License Management

### Generating a License

1. Click "Licenses" in the sidebar
2. Click "Generate License"
3. Select:
   - **Product**: Which product
   - **Customer**: Who it's for
   - **Plan**: Which plan
   - **Type**: Trial, Monthly, Yearly, etc.
   - **Start Date**: When it begins
   - **Max Devices**: Override plan default
   - **Features**: Override plan features
4. Click "Generate"
5. Copy the license key to give to customer

### Bulk Generation

1. Click "Licenses" → "Bulk Generate"
2. Configure:
   - **Product**: Target product
   - **Plan**: License plan
   - **Count**: Number of licenses (1-1000)
   - **Customer**: Optional assignment
   - **Max Devices**: Per license
3. Click "Generate"
4. Export as CSV if needed

### License Actions

| Action | Description | Reversible? |
|--------|-------------|-------------|
| View | See license details | - |
| Edit | Modify license properties | Yes |
| Extend | Change expiry date | Yes |
| Suspend | Temporarily disable | Yes (activate) |
| Revoke | Permanently invalidate | No |
| Reset Devices | Clear device bindings | Yes |
| Copy Key | Copy license key | - |

### Extending a License

1. Open license details
2. Click "Extend"
3. Set new expiry date
4. Enter reason (for audit log)
5. Click "Extend"

### Suspending a License

1. Open license details
2. Click "Suspend"
3. Enter reason
4. Click "Suspend"

**Effect**: License validation fails, but device bindings preserved.

### Revoking a License

1. Open license details
2. Click "Revoke"
3. Enter reason
4. Confirm (this is permanent!)
5. Click "Revoke"

**Effect**: License permanently invalidated. Cannot be undone.

### Resetting Devices

1. Open license details
2. Click "Devices" tab
3. Click "Reset Devices"
4. Confirm

**Effect**: All device bindings removed. Customer can activate on new devices.

## Customer Management

### Creating a Customer

1. Click "Customers" in the sidebar
2. Click "Create Customer"
3. Enter:
   - **Name**: Customer name
   - **Email**: Contact email
   - **Notes**: Optional notes
4. Click "Create"

### Viewing Customer Details

1. Open customer details
2. See all licenses across products
3. View device bindings
4. Check license history

## Device Management

### Viewing Devices

1. Click "Devices" in the sidebar
2. Filter by product, customer, or status
3. See device details

### Resetting a Device

1. Find the device
2. Click "Reset"
3. Confirm

**Effect**: Device binding removed. Customer can activate on new device.

### Suspend vs Remove vs Reset

| Action | Effect | Use Case |
|--------|--------|----------|
| Suspend | Device blocked | Temporary restriction |
| Remove | Device deleted | Cleanup |
| Reset | Binding cleared | Customer needs new device |

## Analytics

### Global Analytics

- View trends across all products
- Activation/validation charts
- License distribution
- Error rates

### Product Analytics

1. Select product from switcher
2. Click "Analytics"
3. View product-specific metrics

### Date Range Selection

Use the date picker to select custom ranges:
- Last 7 days
- Last 30 days
- Last 90 days
- Custom range

## Audit Logs

### Viewing Audit Logs

1. Click "Audit Logs" in the sidebar
2. Filter by:
   - Action type
   - Actor
   - Date range
   - Target type
3. Click entry for details

### What's Logged

- All login attempts
- License creation/modification
- Device registration/reset
- Product changes
- Admin user changes

## Settings

### System Settings

Configure global settings:
- License validation intervals
- Offline grace defaults
- Rate limiting
- API keys

### Admin User Management

1. Click "Settings" → "Users"
2. Create new admin users
3. Assign roles
4. Deactivate users

### Role Permissions

| Role | Can Read | Can Create | Can Edit | Can Revoke | Can Manage Users |
|------|----------|------------|----------|------------|------------------|
| SuperAdmin | ✓ | ✓ | ✓ | ✓ | ✓ |
| Admin | ✓ | ✓ | ✓ | ✓ | ✗ |
| Support | ✓ | ✗ | ✗ | ✗ | ✗ |
| Viewer | ✓ | ✗ | ✗ | ✗ | ✗ |

## Product Switcher

The product switcher in the top bar lets you:

1. View "All Products" for global view
2. Select a specific product
3. All data automatically filters to that product

**Important**: Always check which product is selected before making changes!

## Search

Use the global search to find:
- License keys
- Customer names/emails
- Product names
- Device IDs

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+K | Open search |
| Ctrl+/ | Toggle sidebar |
| Esc | Close modal |

## Troubleshooting

### "Product Mismatch" Error
- Customer is trying to use wrong license key
- Verify license belongs to correct product

### "Device Limit Reached"
- Customer has too many devices
- Reset devices or increase plan limit

### "License Expired"
- Check license expiry date
- Extend license if needed

### "License Revoked"
- License permanently invalidated
- Generate new license for customer

### Cannot Login
- Check username/password
- Account may be locked (too many failed attempts)
- Contact SuperAdmin
