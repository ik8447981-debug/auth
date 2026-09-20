# License Platform - Architecture Document

## System Overview

The Multi-EXE License Management Platform is a centralized system for managing software licenses across multiple Windows EXE applications. It provides product isolation, cryptographic license verification, device binding, and feature entitlement.

## Architecture Principles

1. **Server-Side Authority**: All license validation decisions are made server-side
2. **Product Isolation**: License for Product A cannot be used in Product B
3. **Defense in Depth**: Multiple layers of security validation
4. **Privacy by Design**: Only hashed device fingerprints transmitted, not raw hardware IDs
5. **Audit Everything**: All state changes are logged
6. **Fail Secure**: Deny by default when validation fails

## High-Level Architecture

```
┌─────────────────────────────────────────────────────┐
│                    ADMIN PANEL                       │
│              (React + TypeScript)                    │
│  ┌─────────┐ ┌──────────┐ ┌─────────┐ ┌─────────┐  │
│  │Products │ │ Licenses │ │Customers│ │Analytics│  │
│  └────┬────┘ └────┬─────┘ └────┬────┘ └────┬────┘  │
│       └────────────┴────────────┴────────────┘       │
│                        │ HTTP/HTTPS                  │
└────────────────────────┼────────────────────────────┘
                         │
┌────────────────────────┼────────────────────────────┐
│                   API LAYER                          │
│              (ASP.NET Core 8)                        │
│  ┌──────────┐ ┌───────────┐ ┌──────────────────┐   │
│  │Admin API │ │Client API │ │   Middleware      │   │
│  │(RBAC)    │ │(API Key)  │ │(Rate Limit/Auth) │   │
│  └────┬─────┘ └─────┬─────┘ └────────┬─────────┘   │
│       │              │                 │              │
│  ┌────┴──────────────┴─────────────────┴──────────┐ │
│  │              APPLICATION LAYER                  │ │
│  │    Services / Business Logic / Validation       │ │
│  └───────────────────┬────────────────────────────┘ │
└──────────────────────┼──────────────────────────────┘
                       │
┌──────────────────────┼──────────────────────────────┐
│              INFRASTRUCTURE LAYER                    │
│  ┌──────────┐ ┌──────┴───────┐ ┌────────────────┐  │
│  │PostgreSQL│ │ Entity Frame │ │  Security      │  │
│  │ Database │ │     Core     │ │  (RSA Signing) │  │
│  └──────────┘ └──────────────┘ └────────────────┘  │
└─────────────────────────────────────────────────────┘
                       │
┌──────────────────────┼──────────────────────────────┐
│                CLIENT SDK                            │
│           (C# Class Library)                         │
│  ┌──────────┐ ┌──────────┐ ┌───────────────────┐   │
│  │Activation│ │Validation│ │ Feature Entitlem. │   │
│  └──────────┘ └──────────┘ └───────────────────┘   │
│  ┌──────────┐ ┌──────────┐ ┌───────────────────┐   │
│  │  Cache   │ │Signature │ │ Device Identity   │   │
│  │ (DPAPI)  │ │Verifier  │ │ (Privacy-Safe)    │   │
│  └──────────┘ └──────────┘ └───────────────────┘   │
└─────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### 1. Domain Layer (LicensePlatform.Domain)
- Entities with business rules
- Enums for type safety
- No external dependencies
- Pure C# domain model

### 2. Shared Layer (LicensePlatform.Shared)
- DTOs for API contracts
- Request/Response models
- Constants and error codes
- Security models (SignedLicenseResponse)

### 3. Infrastructure Layer (LicensePlatform.Infrastructure)
- Entity Framework Core DbContext
- Repository implementations
- Database migrations
- External service integrations

### 4. Security Layer (LicensePlatform.Security)
- RSA key pair generation
- License key generation
- Device fingerprint computation
- License response signing
- Cryptographic operations

### 5. Application Layer (LicensePlatform.Application)
- Business logic services
- License validation rules
- Product isolation enforcement
- Audit logging
- Analytics computation

### 6. API Layer (LicensePlatform.Api)
- REST controllers
- Authentication/Authorization middleware
- Rate limiting
- Request logging
- Error handling

### 7. Client SDK (LicenseClient.SDK)
- License activation
- Online/offline validation
- Local cache with DPAPI
- Signature verification
- Feature entitlement checks
- Device identity generation

## Security Architecture

### Authentication Flow (Admin)
```
Admin Login → BCrypt Verify → JWT Token → RBAC Authorization
```

### License Activation Flow
```
Client → HTTPS POST /api/v1/client/activate
      → Server validates ProductId
      → Server validates LicenseKey format
      → Server looks up License
      → Server checks status (not revoked/expired)
      → Server checks ProductId match
      → Server computes device fingerprint
      → Server checks device limit
      → Server registers device
      → Server signs response with RSA-2048
      → Client verifies signature
      → Client stores in DPAPI cache
```

### Signature Verification
```
License Response → Canonical Serialization → SHA256 Hash
                                                         ↓
Private Key → RSA Sign → Signature              Public Key → RSA Verify
```

### Product Isolation
Every activation/validation request includes ProductId.
Server-side validation:
1. License exists for this ProductId
2. Request's ProductId matches License's ProductId
3. Product is Active (not Disabled/Archived)

Cross-product usage returns: PRODUCT_MISMATCH

### Key Rotation
- Multiple signing keys supported (KeyVersion tracking)
- Old keys can be deactivated
- License responses include KeyVersion
- Client can verify with any active key

## Data Model

### Core Entities
- **Product**: Software application (Cleaner.exe, Optimizer.exe)
- **LicensePlan**: Pricing/duration tier per product
- **Customer**: End user
- **License**: Entitlement record
- **LicenseDevice**: Device binding
- **ProductFeature**: Feature definition
- **LicenseFeature**: Feature entitlement per license
- **ValidationEvent**: Audit trail for validations
- **SigningKey**: RSA key pairs for signing
- **AdminUser**: System administrators
- **AuditLog**: System-wide audit trail

### Relationships
```
Product 1──N LicensePlan
Product 1──N ProductFeature
Product 1──N ProductVersion
Product 1──N License
LicensePlan 1──N License
LicensePlan N──N PlanFeature
Customer 1──N License
License 1──N LicenseDevice
License N──N LicenseFeature
License 1──N ValidationEvent
ProductFeature N──N PlanFeature
ProductFeature N──N LicenseFeature
```

## API Design

### Versioned API
All endpoints use `/api/v1/` prefix for versioning.

### Client Endpoints (API Key Auth)
- `POST /api/v1/client/activate`
- `POST /api/v1/client/validate`
- `POST /api/v1/client/deactivate`
- `POST /api/v1/client/heartbeat`

### Admin Endpoints (JWT Auth + RBAC)
- Products: CRUD + features
- Licenses: CRUD + bulk + extend/suspend/revoke
- Customers: CRUD + license history
- Devices: list + reset + suspend
- Plans: CRUD per product
- Analytics: global + per product
- Audit Logs: query + filter
- Auth: login + user management

## Deployment Architecture

```
┌─────────────────────────────────────────┐
│              Reverse Proxy              │
│            (Nginx / IIS)               │
│                  │ TLS                  │
└──────────────────┼─────────────────────┘
                   │
┌──────────────────┼─────────────────────┐
│           ASP.NET Core API              │
│          (Docker / IIS)                 │
└──────────────────┼─────────────────────┘
                   │
┌──────────────────┼─────────────────────┐
│           PostgreSQL Database           │
│          (Docker / RDS)                 │
└─────────────────────────────────────────┘
```

### Environment Configuration
- **Development**: Local PostgreSQL, self-signed certs
- **Staging**: Mirrors production, test data
- **Production**: Hardened, monitoring, backups
