# License Platform - Database Schema

## Overview

PostgreSQL database with Entity Framework Core migrations. All tables use UUID primary keys.

## Tables

### Users (Admin Users)
```sql
CREATE TABLE AdminUsers (
    AdminUserId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Username VARCHAR(100) UNIQUE NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role INTEGER NOT NULL DEFAULT 3, -- Viewer
    IsActive BOOLEAN NOT NULL DEFAULT true,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    LastLoginAt TIMESTAMP WITH TIME ZONE,
    FailedLoginAttempts INTEGER NOT NULL DEFAULT 0,
    LockedUntil TIMESTAMP WITH TIME ZONE
);
```

### Products
```sql
CREATE TABLE Products (
    ProductId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ProductName VARCHAR(200) UNIQUE NOT NULL,
    DisplayName VARCHAR(200) NOT NULL,
    Description TEXT,
    ProductCode VARCHAR(50) UNIQUE NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0, -- Active
    CurrentVersion VARCHAR(50),
    MinimumSupportedVersion VARCHAR(50),
    LatestVersion VARCHAR(50),
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    IsDeleted BOOLEAN NOT NULL DEFAULT false
);

CREATE INDEX idx_products_code ON Products(ProductCode);
CREATE INDEX idx_products_status ON Products(Status) WHERE IsDeleted = false;
```

### ProductFeatures
```sql
CREATE TABLE ProductFeatures (
    ProductFeatureId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ProductId UUID NOT NULL REFERENCES Products(ProductId),
    FeatureKey VARCHAR(100) NOT NULL,
    FeatureName VARCHAR(200) NOT NULL,
    Description TEXT,
    IsEnabled BOOLEAN NOT NULL DEFAULT true,
    
    UNIQUE(ProductId, FeatureKey)
);
```

### ProductVersions
```sql
CREATE TABLE ProductVersions (
    ProductVersionId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ProductId UUID NOT NULL REFERENCES Products(ProductId),
    VersionNumber VARCHAR(50) NOT NULL,
    IsBlocked BOOLEAN NOT NULL DEFAULT false,
    ReleasedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    Notes TEXT,
    
    UNIQUE(ProductId, VersionNumber)
);
```

### LicensePlans
```sql
CREATE TABLE LicensePlans (
    LicensePlanId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ProductId UUID NOT NULL REFERENCES Products(ProductId),
    PlanName VARCHAR(100) NOT NULL,
    DurationDays INTEGER NOT NULL DEFAULT 30, -- -1 for lifetime
    MaxDevices INTEGER NOT NULL DEFAULT 1,
    OfflineGraceHours INTEGER NOT NULL DEFAULT 24,
    ValidationIntervalMinutes INTEGER NOT NULL DEFAULT 60,
    Status INTEGER NOT NULL DEFAULT 0, -- Active
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    UNIQUE(ProductId, PlanName)
);
```

### PlanFeatures
```sql
CREATE TABLE PlanFeatures (
    PlanFeatureId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    PlanId UUID NOT NULL REFERENCES LicensePlans(LicensePlanId),
    FeatureId UUID NOT NULL REFERENCES ProductFeatures(ProductFeatureId),
    
    UNIQUE(PlanId, FeatureId)
);
```

### Customers
```sql
CREATE TABLE Customers (
    CustomerId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(200) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0, -- Active
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    Notes TEXT
);

CREATE INDEX idx_customers_email ON Customers(Email);
```

### Licenses
```sql
CREATE TABLE Licenses (
    LicenseId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    LicenseKey VARCHAR(50) UNIQUE NOT NULL,
    ProductId UUID NOT NULL REFERENCES Products(ProductId),
    CustomerId UUID NOT NULL REFERENCES Customers(CustomerId),
    PlanId UUID NOT NULL REFERENCES LicensePlans(LicensePlanId),
    Status INTEGER NOT NULL DEFAULT 0, -- Created
    Type INTEGER NOT NULL DEFAULT 5, -- Yearly
    StartDate TIMESTAMP WITH TIME ZONE NOT NULL,
    ExpiryDate TIMESTAMP WITH TIME ZONE, -- NULL for lifetime
    MaxDevices INTEGER NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    ActivatedAt TIMESTAMP WITH TIME ZONE,
    SuspendedAt TIMESTAMP WITH TIME ZONE,
    RevokedAt TIMESTAMP WITH TIME ZONE,
    RevokeReason TEXT
);

CREATE INDEX idx_licenses_key ON Licenses(LicenseKey);
CREATE INDEX idx_licenses_product ON Licenses(ProductId);
CREATE INDEX idx_licenses_customer ON Licenses(CustomerId);
CREATE INDEX idx_licenses_status ON Licenses(Status);
```

### LicenseFeatures
```sql
CREATE TABLE LicenseFeatures (
    LicenseFeatureId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    LicenseId UUID NOT NULL REFERENCES Licenses(LicenseId),
    FeatureId UUID NOT NULL REFERENCES ProductFeatures(ProductFeatureId),
    
    UNIQUE(LicenseId, FeatureId)
);
```

### LicenseDevices
```sql
CREATE TABLE LicenseDevices (
    LicenseDeviceId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    LicenseId UUID NOT NULL REFERENCES Licenses(LicenseId),
    ProductId UUID NOT NULL REFERENCES Products(ProductId),
    CustomerId UUID NOT NULL REFERENCES Customers(CustomerId),
    DeviceFingerprint VARCHAR(255) NOT NULL,
    DeviceName VARCHAR(200),
    FirstSeen TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    LastSeen TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    ActivationDate TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    Status INTEGER NOT NULL DEFAULT 0, -- Active
    ApplicationVersion VARCHAR(50),
    OSVersion VARCHAR(100),
    
    UNIQUE(LicenseId, DeviceFingerprint)
);

CREATE INDEX idx_devices_license ON LicenseDevices(LicenseId);
CREATE INDEX idx_devices_fingerprint ON LicenseDevices(DeviceFingerprint);
```

### ValidationEvents
```sql
CREATE TABLE ValidationEvents (
    ValidationEventId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    LicenseId UUID NOT NULL REFERENCES Licenses(LicenseId),
    ProductId UUID NOT NULL REFERENCES Products(ProductId),
    DeviceFingerprint VARCHAR(255) NOT NULL,
    EventType INTEGER NOT NULL, -- Activation, Validation, etc.
    Success BOOLEAN NOT NULL,
    ErrorCode VARCHAR(100),
    ServerTimestamp TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    ClientTimestamp TIMESTAMP WITH TIME ZONE,
    IpAddress VARCHAR(45),
    UserAgent TEXT
);

CREATE INDEX idx_validations_license ON ValidationEvents(LicenseId);
CREATE INDEX idx_validations_product ON ValidationEvents(ProductId);
CREATE INDEX idx_validations_timestamp ON ValidationEvents(ServerTimestamp);
```

### AuditLogs
```sql
CREATE TABLE AuditLogs (
    AuditLogId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ActorId UUID REFERENCES AdminUsers(AdminUserId),
    ActorName VARCHAR(200),
    Action INTEGER NOT NULL,
    TargetType VARCHAR(100),
    TargetId VARCHAR(100),
    Timestamp TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    IpAddress VARCHAR(45),
    Result VARCHAR(50) NOT NULL DEFAULT 'success',
    Metadata TEXT, -- JSON
    RequestId VARCHAR(100)
);

CREATE INDEX idx_auditlogs_actor ON AuditLogs(ActorId);
CREATE INDEX idx_auditlogs_action ON AuditLogs(Action);
CREATE INDEX idx_auditlogs_target ON AuditLogs(TargetType, TargetId);
CREATE INDEX idx_auditlogs_timestamp ON AuditLogs(Timestamp);
```

### SigningKeys
```sql
CREATE TABLE SigningKeys (
    SigningKeyId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    KeyVersion INTEGER UNIQUE NOT NULL,
    PublicKeyPem TEXT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT true,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    ActivatedAt TIMESTAMP WITH TIME ZONE,
    DeactivatedAt TIMESTAMP WITH TIME ZONE
);
```

### SystemSettings
```sql
CREATE TABLE SystemSettings (
    SystemSettingId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    SettingKey VARCHAR(100) UNIQUE NOT NULL,
    SettingValue TEXT NOT NULL,
    Description TEXT,
    UpdatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);
```

### ApiClients
```sql
CREATE TABLE ApiClients (
    ApiClientId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ClientName VARCHAR(200) NOT NULL,
    ApiKey VARCHAR(100) UNIQUE NOT NULL,
    ApiSecret VARCHAR(255) NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT true,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    LastUsedAt TIMESTAMP WITH TIME ZONE,
    Permissions TEXT -- JSON array
);
```

## Indexes Summary

| Table | Index | Purpose |
|-------|-------|---------|
| Products | ProductCode | Lookup by code |
| Products | Status (filtered) | Active product queries |
| ProductFeatures | (ProductId, FeatureKey) | Feature lookup |
| LicensePlans | (ProductId, PlanName) | Plan lookup |
| Licenses | LicenseKey | Key lookup |
| Licenses | ProductId | Product-scoped queries |
| Licenses | CustomerId | Customer license lookup |
| Licenses | Status | Status filtering |
| LicenseDevices | LicenseId | Device listing |
| LicenseDevices | DeviceFingerprint | Device lookup |
| ValidationEvents | LicenseId | Validation history |
| ValidationEvents | ServerTimestamp | Time-range queries |
| AuditLogs | Action | Action filtering |
| AuditLogs | (TargetType, TargetId) | Target lookup |
| AuditLogs | Timestamp | Time-range queries |

## Soft Delete

Only `Products` table uses soft delete via `IsDeleted` column. All queries filter by `IsDeleted = false` by default.

## Timestamps

All tables with `CreatedAt` and `UpdatedAt` use `TIMESTAMP WITH TIME ZONE` with `NOW()` defaults.
