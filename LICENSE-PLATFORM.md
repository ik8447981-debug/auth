# Multi-EXE License Management Platform

## Architecture Overview

```
LICENSE PLATFORM
│
├── Products / Apps
│   ├── App 01 (Cleaner.exe)
│   ├── App 02 (Optimizer.exe)
│   └── App 03 (Downloader.exe)
│
├── Customers
├── License Plans
├── Global Devices
├── Analytics
├── Audit Logs
├── Administrators
└── System Settings
```

## Tech Stack

- **Backend**: C# / .NET 8 / ASP.NET Core Web API
- **Database**: PostgreSQL with Entity Framework Core
- **Client SDK**: C# / .NET 8 Class Library
- **Admin Panel**: React + TypeScript + Tailwind CSS
- **Security**: RSA-2048 asymmetric signing, BCrypt password hashing
- **Testing**: xUnit, Moq, IntegrationTests

## Project Structure

```
/license-platform
    /server
        /LicensePlatform.Api           - Entry point, controllers, middleware
        /LicensePlatform.Application   - Business logic, services, validators
        /LicensePlatform.Domain        - Entities, value objects, interfaces
        /LicensePlatform.Infrastructure - DB, caching, external services
        /LicensePlatform.Security      - Crypto, signing, key management
        /LicensePlatform.Tests         - Unit & integration tests
    /client-sdk
        /LicenseClient.SDK             - Client SDK for EXE integration
        /LicenseClient.Tests
    /admin-panel                       - React admin dashboard
    /shared
        /LicensePlatform.Shared        - Shared contracts, DTOs
    /docs
    /deployment
```

## Security Model

- Server-side private key NEVER in client EXE
- Client receives ONLY public verification key
- All activation/validation via HTTPS
- Cryptographically signed license responses
- RBAC enforced server-side
- Audit logs for all actions
