# License Platform - Security Document

## Security Architecture

The License Platform implements defense-in-depth security with server-side authority as the core principle.

## Core Security Principles

1. **Server-Side Authority**: All license decisions are made server-side
2. **Never Trust the Client**: Client-provided data is validated, not trusted
3. **Least Privilege**: RBAC with minimal necessary permissions
4. **Defense in Depth**: Multiple security layers
5. **Fail Secure**: Deny by default
6. **Audit Everything**: All state changes logged

## Cryptographic Operations

### License Key Generation
- Cryptographically secure random number generator (CSPRNG)
- Format: `PRODUCTCODE-XXXX-XXXX-XXXX` (16 alphanumeric chars)
- Collision detection with database unique constraint
- No predictable sequences, timestamps, or usernames

### License Response Signing
- Algorithm: RSA-2048 with SHA-256
- Private key: Server-side only, never in client
- Public key: Embedded in client SDK configuration
- Canonical serialization for deterministic signing
- Key versioning for rotation support

### Key Rotation
- Multiple active signing keys supported
- KeyVersion field in license responses
- Old keys can be deactivated
- Migration period for key transitions

### Device Fingerprinting
- Privacy-conscious design
- System signals → Normalize → SHA256 hash → Canonical representation
- Only hash transmitted to server, never raw hardware IDs
- Constant-time comparison for security

## Authentication

### Admin Authentication
- Password hashing: BCrypt with work factor 12
- JWT tokens with configurable expiry
- Refresh token support
- Account lockout after 5 failed attempts
- Login rate limiting

### Client Authentication
- API key-based authentication
- API key transmitted in X-Api-Key header
- API secret stored server-side only
- Per-client rate limiting

## Authorization (RBAC)

### Roles
| Role | Permissions |
|------|------------|
| SuperAdmin | Full access, can manage admins |
| Admin | Products, Licenses, Customers, Devices, Analytics |
| Support | Read-only access, can view but not modify |
| Viewer | Read-only access to dashboards |

### Permissions
```
products.read, products.write
licenses.read, licenses.create, licenses.edit, licenses.revoke
devices.reset
customers.read, customers.write
analytics.read
audit.read
settings.write
```

### Enforcement
- Server-side authorization on every request
- JWT claims include role
- Middleware validates permissions
- No client-side-only checks

## Rate Limiting

### Endpoints
| Endpoint Type | Limit | Window |
|---------------|-------|--------|
| Login | 5 requests | 1 minute |
| Activation | 10 requests | 1 minute |
| Validation | 60 requests | 1 minute |
| Admin API | 100 requests | 1 minute |
| Client API | 30 requests | 1 minute |

### Implementation
- Per-IP rate limiting
- In-memory sliding window
- X-RateLimit-Remaining header
- 429 Too Many Requests response

## Input Validation

### Server-Side
- All input validated with data annotations
- Request body deserialization validation
- Query parameter validation
- SQL injection prevention (EF Core parameterized queries)
- XSS prevention (output encoding)

### Client-Side
- License key format validation before submission
- Device fingerprint validation
- Response schema validation

## Output Validation

### License Response
- Cryptographic signature verification
- Response schema validation
- Time consistency checks
- Nonce verification (replay protection)

## Transport Security

- HTTPS/TLS required for all endpoints
- HSTS headers enabled
- Secure cookie flags
- CORS policy restricts origins

## Secret Management

### Server-Side
- Database credentials in environment variables
- JWT secret in secure configuration
- RSA private keys in secure storage (never in code)
- API secrets hashed in database

### Client-Side
- Public verification key only
- No private key material
- No database credentials
- No admin secrets
- DPAPI for local cache protection

## Audit Logging

### Logged Events
- All authentication attempts
- All license state changes
- All device registrations/resets
- All admin actions
- All system configuration changes

### Log Format
```
Actor | Action | Target | Timestamp | IP | Result | Metadata
```

### Security
- Secrets never logged
- Passwords never logged
- License keys logged with masking
- Tamper-evident logging

## Product Isolation

### Mechanism
1. Every activation request includes ProductId
2. Server validates License belongs to this ProductId
3. Cross-product usage returns PRODUCT_MISMATCH
4. ProductId in signed response prevents tampering
5. Client validates ProductId matches expected value

### Guarantee
A license for Product A will NEVER work in Product B, because:
- Server validates ProductId match
- Signed response includes ProductId
- Client verifies ProductId before use

## Offline Security

### Grace Period
- Configurable per plan (0-720 hours)
- Signed cached license state
- DPAPI protection for local storage
- Clock rollback detection

### Limitations
- Offline grace is finite
- No infinite offline trust
- Periodic revalidation required
- Clock manipulation detected

## Anti-Tampering

### Signed Responses
- License responses cryptographically signed
- Client verifies signature before use
- Canonical serialization prevents signature bypass
- Key version tracking

### Local Storage
- DPAPI-encrypted cache
- Tamper detection
- Automatic invalidation on corruption

## Security Headers

```
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
Content-Security-Policy: default-src 'self'
```

## Dependency Security

- Regular dependency updates
- Vulnerability scanning
- Known CVE monitoring
- Minimal dependency footprint

## Incident Response

### Detection
- Failed login monitoring
- Rate limit violations
- Signature verification failures
- Unusual activation patterns

### Response
- Automatic account lockout
- Rate limit enforcement
- Audit log analysis
- Manual review triggers
