# Multi-EXE License Management Platform

A production-grade, centralized license management system for managing multiple Windows EXE applications with product isolation, cryptographic verification, and device binding.

## Features

- **Multi-Product Support**: Manage unlimited products (Cleaner.exe, Optimizer.exe, etc.)
- **Product Isolation**: License for Product A cannot be used in Product B
- **Cryptographic Signing**: RSA-2048 signed license responses
- **Device Binding**: HWID-based device fingerprinting with configurable limits
- **Feature Entitlement**: Per-feature unlock based on license plan
- **Offline Grace**: Configurable offline validation period
- **Admin Dashboard**: Modern React-based admin panel
- **Client SDK**: Easy C# integration for EXE developers
- **RBAC**: Role-based access control (SuperAdmin, Admin, Support, Viewer)
- **Audit Logging**: Complete audit trail for all actions
- **Analytics**: Real-time dashboards and metrics

## Architecture

```
┌─────────────────────────────────────────────────┐
│                 ADMIN PANEL                      │
│            (React + TypeScript)                  │
└─────────────────────┬───────────────────────────┘
                      │ HTTPS
┌─────────────────────┼───────────────────────────┐
│                 API LAYER                        │
│           (ASP.NET Core 8)                       │
│  ┌──────────┐ ┌───────────┐ ┌──────────────┐   │
│  │Admin API │ │Client API │ │  Middleware   │   │
│  └──────────┘ └───────────┘ └──────────────┘   │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────┼───────────────────────────┐
│           APPLICATION LAYER                      │
│      (Services / Business Logic)                 │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────┼───────────────────────────┐
│          INFRASTRUCTURE LAYER                    │
│    (PostgreSQL + Entity Framework Core)          │
└─────────────────────────────────────────────────┘
```

## Quick Start

### Prerequisites

- .NET 8 SDK
- PostgreSQL 14+
- Node.js 18+ (for admin panel)

### 1. Setup Database

```bash
# Create database
createdb license_platform

# Update connection string
# Edit server/LicensePlatform.Api/appsettings.json
```

### 2. Run API

```bash
cd server
dotnet restore
dotnet run --project LicensePlatform.Api
```

API available at: `http://localhost:5000`
Swagger: `http://localhost:5000/swagger`

### 3. Run Admin Panel

```bash
cd admin-panel
npm install
npm run dev
```

Admin panel: `http://localhost:5173`

### 4. Login

- Username: `admin`
- Password: `admin123`

## Project Structure

```
license-platform/
├── server/
│   ├── LicensePlatform.Api/          # API controllers, middleware
│   ├── LicensePlatform.Application/  # Business logic services
│   ├── LicensePlatform.Domain/       # Entities, enums
│   ├── LicensePlatform.Infrastructure/ # DB, repositories
│   ├── LicensePlatform.Security/     # Crypto, signing
│   └── LicensePlatform.Tests/        # Unit & integration tests
├── client-sdk/
│   ├── LicenseClient.SDK/            # Client SDK for EXEs
│   └── LicenseClient.Tests/          # SDK tests
├── admin-panel/                      # React admin dashboard
├── docs/                             # Documentation
└── deployment/                       # Docker, scripts
```

## Integration Example

```csharp
// In your EXE application
var config = new LicenseConfiguration
{
    ServerUrl = "https://license.yourdomain.com",
    ProductId = "CLEANER",
    ProductCode = "CLNR",
    PublicKeyPem = "-----BEGIN PUBLIC KEY-----\n..."
};

services.AddLicenseClient(config);

// Activate
var result = await licenseClient.ActivateAsync("CLNR-7K4P-X92M-Q8ZT");

// Check features
if (licenseClient.HasFeature("advanced_cleanup"))
{
    EnableAdvancedCleanup();
}
```

## Documentation

- [Architecture](docs/ARCHITECTURE.md) - System design and principles
- [API Documentation](docs/API.md) - REST API reference
- [Database Schema](docs/DATABASE.md) - Table structure and indexes
- [Security](docs/SECURITY.md) - Security architecture and practices
- [Client SDK](docs/CLIENT-SDK.md) - SDK integration guide
- [Admin Guide](docs/ADMIN-GUIDE.md) - Admin panel usage
- [Product Guide](docs/PRODUCT-GUIDE.md) - EXE integration guide
- [Deployment](docs/DEPLOYMENT.md) - Production deployment
- [Troubleshooting](docs/TROUBLESHOOTING.md) - Common issues and solutions

## License Key Format

```
PRODUCTCODE-XXXX-XXXX-XXXX

Example:
CLNR-7K4P-X92M-Q8ZT
OPTM-3F8N-5P2V-W9KJ
```

## API Endpoints

### Client API
- `POST /api/v1/client/activate` - Activate license
- `POST /api/v1/client/validate` - Validate license
- `POST /api/v1/client/deactivate` - Deactivate license
- `POST /api/v1/client/heartbeat` - Server heartbeat

### Admin API
- Products: CRUD + features
- Licenses: CRUD + bulk + extend/suspend/revoke
- Customers: CRUD + license history
- Devices: list + reset + suspend
- Plans: CRUD per product
- Analytics: global + per product
- Audit Logs: query + filter

## Security

- Server-side authority for all license decisions
- RSA-2048 cryptographic signing
- BCrypt password hashing
- JWT authentication
- RBAC authorization
- Rate limiting
- Audit logging
- DPAPI for local cache protection

## Testing

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "ProductIsolation"

# Run integration tests
dotnet test --filter "Category=Integration"
```

## Deployment

See [Deployment Guide](docs/DEPLOYMENT.md) for:
- Docker deployment
- IIS/Windows deployment
- Linux systemd deployment
- SSL/TLS configuration
- Database backup

## Contributing

1. Fork the repository
2. Create feature branch
3. Commit changes
4. Push to branch
5. Create Pull Request

## Support

- Documentation: `/docs`
- Issues: GitHub Issues
- Email: support@yourdomain.com

## License

MIT License - see LICENSE file for details
