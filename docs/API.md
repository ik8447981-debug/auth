# License Platform - API Documentation

## Base URL
```
https://your-domain.com/api/v1
```

## Authentication

### Admin Endpoints
All admin endpoints require JWT Bearer token.
```
Authorization: Bearer <token>
```

### Client Endpoints
Client endpoints require API key.
```
X-Api-Key: <api-key>
```

## Common Response Format

### Success
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation completed",
  "requestId": "uuid"
}
```

### Error
```json
{
  "success": false,
  "error": "ERROR_CODE",
  "message": "Human readable message",
  "requestId": "uuid"
}
```

## Admin Authentication

### POST /api/v1/admin/auth/login
Login as administrator.

**Request:**
```json
{
  "username": "admin",
  "password": "securepassword"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "jwt-token",
    "expiresAt": "2026-12-01T00:00:00Z",
    "role": "SuperAdmin"
  }
}
```

## Products

### GET /api/v1/admin/products
List all products with pagination.

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20)

### POST /api/v1/admin/products
Create a new product. Requires Admin or SuperAdmin role.

**Request:**
```json
{
  "productName": "Cleaner",
  "displayName": "Cleaner Pro",
  "description": "System cleaning utility",
  "productCode": "CLEANER"
}
```

### GET /api/v1/admin/products/{id}
Get product details including features, plans, and versions.

### PUT /api/v1/admin/products/{id}
Update product. Requires Admin or SuperAdmin role.

### DELETE /api/v1/admin/products/{id}
Delete/archive product. Requires SuperAdmin role only.

### GET /api/v1/admin/products/{id}/features
Get all features for a product.

### POST /api/v1/admin/products/{id}/features
Add a feature to a product.

**Request:**
```json
{
  "featureKey": "advanced_cleanup",
  "featureName": "Advanced Cleanup",
  "description": "Deep system cleaning"
}
```

### PUT /api/v1/admin/products/{id}/features/{featureId}
Toggle feature enabled/disabled.

### DELETE /api/v1/admin/products/{id}/features/{featureId}
Remove a feature from a product.

## License Plans

### GET /api/v1/admin/plans/product/{productId}
Get all plans for a specific product.

### POST /api/v1/admin/plans
Create a new license plan.

**Request:**
```json
{
  "productId": "uuid",
  "planName": "Professional",
  "durationDays": 365,
  "maxDevices": 3,
  "offlineGraceHours": 72,
  "validationIntervalMinutes": 60,
  "featureIds": ["uuid1", "uuid2"]
}
```

### PUT /api/v1/admin/plans/{id}
Update a plan.

### DELETE /api/v1/admin/plans/{id}
Delete a plan.

## Licenses

### GET /api/v1/admin/licenses
List licenses with filtering and pagination.

**Query Parameters:**
- `page` (int)
- `pageSize` (int)
- `productId` (Guid, optional)
- `status` (enum, optional): Created, Active, Expired, Suspended, Revoked, Disabled
- `search` (string, optional): search by key, customer name, or email

### POST /api/v1/admin/licenses
Generate a single license.

**Request:**
```json
{
  "productId": "uuid",
  "customerId": "uuid",
  "planId": "uuid",
  "licenseType": "Yearly",
  "startDate": "2026-01-01T00:00:00Z",
  "maxDevices": 2,
  "featureIds": ["uuid1"]
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "licenseId": "uuid",
    "licenseKey": "CLNR-7K4P-X92M-Q8ZT",
    "status": "Created",
    "expiresAt": "2027-01-01T00:00:00Z"
  }
}
```

### POST /api/v1/admin/licenses/bulk
Bulk generate licenses (up to 1000).

**Request:**
```json
{
  "productId": "uuid",
  "planId": "uuid",
  "count": 50,
  "customerId": "uuid (optional)",
  "maxDevices": 1,
  "featureIds": []
}
```

### PUT /api/v1/admin/licenses/{id}/extend
Extend license expiry.

**Request:**
```json
{
  "newExpiryDate": "2028-01-01T00:00:00Z",
  "reason": "Customer renewal"
}
```

### PUT /api/v1/admin/licenses/{id}/suspend
Suspend a license (temporary disable).

**Request:**
```json
{
  "reason": "Payment dispute"
}
```

### PUT /api/v1/admin/licenses/{id}/revoke
Permanently revoke a license. Cannot be undone.

**Request:**
```json
{
  "reason": "Terms violation"
}
```

### PUT /api/v1/admin/licenses/{id}/activate
Activate a created license.

### POST /api/v1/admin/licenses/export
Export licenses as CSV. Same filter parameters as GET.

## Customers

### GET /api/v1/admin/customers
List customers with search and pagination.

**Query Parameters:**
- `page`, `pageSize`, `search`

### POST /api/v1/admin/customers
Create a new customer.

**Request:**
```json
{
  "name": "Rahim",
  "email": "rahim@example.com",
  "notes": "VIP customer"
}
```

### GET /api/v1/admin/customers/{id}
Get customer details.

### PUT /api/v1/admin/customers/{id}
Update customer.

### DELETE /api/v1/admin/customers/{id}
Delete customer.

### GET /api/v1/admin/customers/{id}/licenses
Get all licenses for a customer, optionally filtered by product.

**Query Parameters:**
- `productId` (Guid, optional)

## Devices

### GET /api/v1/admin/devices
List devices with filters.

### POST /api/v1/admin/devices/{id}/reset
Reset device binding. Customer can activate on new device.

### POST /api/v1/admin/devices/{id}/suspend
Suspend a device.

### DELETE /api/v1/admin/devices/{id}
Remove a device.

## Analytics

### GET /api/v1/admin/analytics/global
Get global analytics.

**Response:**
```json
{
  "totalProducts": 5,
  "totalLicenses": 1250,
  "activeLicenses": 890,
  "expiredLicenses": 300,
  "revokedLicenses": 60,
  "activeDevices": 1200,
  "activationsToday": 45,
  "validationsToday": 8900,
  "apiErrorsToday": 12
}
```

### GET /api/v1/admin/analytics/product/{productId}
Get product-specific analytics.

### GET /api/v1/admin/analytics/activations
Get activations by day.

**Query Parameters:**
- `from`, `to` (DateTime)
- `productId` (Guid, optional)

### GET /api/v1/admin/analytics/validations
Get validations by day.

## Audit Logs

### GET /api/v1/admin/audit-logs
List audit logs with filters.

**Query Parameters:**
- `page`, `pageSize`
- `action` (AuditAction enum)
- `targetType` (string)
- `from`, `to` (DateTime)

### GET /api/v1/admin/audit-logs/target/{targetType}/{targetId}
Get audit logs for a specific target.

## Client API

### POST /api/v1/client/activate
Activate a license on a device.

**Headers:**
```
X-Api-Key: <api-key>
```

**Request:**
```json
{
  "licenseKey": "CLNR-7K4P-X92M-Q8ZT",
  "deviceId": "hashed-device-fingerprint",
  "applicationVersion": "1.2.0",
  "osVersion": "Windows 11"
}
```

**Response (signed):**
```json
{
  "success": true,
  "licenseId": "uuid",
  "status": "Active",
  "expiresAt": "2027-01-01T00:00:00Z",
  "features": ["basic_cleanup", "advanced_cleanup"],
  "deviceId": "device-uuid",
  "signedLicenseState": "...",
  "serverTime": "2026-09-20T12:00:00Z",
  "keyVersion": 1,
  "nonce": "random-nonce",
  "signature": "rsa-signature"
}
```

### POST /api/v1/client/validate
Validate license (periodic check).

**Request:**
```json
{
  "licenseKey": "CLNR-7K4P-X92M-Q8ZT",
  "deviceId": "hashed-device-fingerprint",
  "applicationVersion": "1.2.0",
  "nonce": "client-generated-nonce"
}
```

### POST /api/v1/client/deactivate
Deactivate license on this device.

### POST /api/v1/client/heartbeat
Periodic heartbeat to check server connectivity.

## Error Codes

| Code | Description |
|------|-------------|
| `INVALID_LICENSE` | License key format invalid |
| `LICENSE_NOT_FOUND` | License not found in database |
| `LICENSE_EXPIRED` | License has expired |
| `LICENSE_REVOKED` | License has been permanently revoked |
| `LICENSE_SUSPENDED` | License is temporarily suspended |
| `PRODUCT_MISMATCH` | License is for a different product |
| `DEVICE_LIMIT_REACHED` | Maximum devices reached |
| `DEVICE_NOT_AUTHORIZED` | Device not registered to this license |
| `INVALID_SIGNATURE` | Cryptographic signature verification failed |
| `SERVER_UNAVAILABLE` | Server is temporarily unavailable |
| `OFFLINE_GRACE_EXPIRED` | Offline grace period has expired |
| `CLOCK_ROLLBACK_DETECTED` | Local clock manipulation detected |
| `RATE_LIMITED` | Too many requests |
| `INVALID_REQUEST` | Request validation failed |
| `UNAUTHORIZED` | Authentication required |
| `FORBIDDEN` | Insufficient permissions |
