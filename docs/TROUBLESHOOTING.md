# License Platform - Troubleshooting Guide

## Common Issues

### 1. Cannot Connect to API

**Symptoms:**
- "Server Unavailable" error
- Connection timeout
- API not responding

**Solutions:**
```bash
# Check if API is running
curl http://localhost:5000/health

# Check API logs
docker logs license-api

# Check firewall
sudo ufw status

# Check port is open
netstat -tlnp | grep 5000
```

### 2. Database Connection Issues

**Symptoms:**
- "Could not connect to database"
- Migration errors
- Timeout exceptions

**Solutions:**
```bash
# Test PostgreSQL connection
psql -h localhost -U postgres -d license_platform

# Check PostgreSQL status
sudo systemctl status postgresql

# Verify connection string
cat appsettings.json | grep ConnectionStrings

# Restart PostgreSQL
sudo systemctl restart postgresql
```

### 3. Migration Errors

**Symptoms:**
- "Pending model changes"
- Migration conflicts
- Schema mismatch

**Solutions:**
```bash
# List migrations
dotnet ef migrations list

# Create new migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update

# Reset and reapply
dotnet ef database drop
dotnet ef database update
```

### 4. Authentication Failures

**Symptoms:**
- "Invalid credentials"
- JWT validation errors
- 401 Unauthorized

**Solutions:**
```bash
# Check JWT secret
echo $JwtSettings__SecretKey

# Verify token format
# Decode JWT at https://jwt.io

# Check user exists
psql -c "SELECT Username FROM AdminUsers"

# Reset admin password
psql -c "UPDATE AdminUsers SET PasswordHash = '$2a$12$...' WHERE Username = 'admin'"
```

### 5. License Activation Fails

**Symptoms:**
- "PRODUCT_MISMATCH"
- "LICENSE_NOT_FOUND"
- "DEVICE_LIMIT_REACHED"

**Solutions:**
```bash
# Check product exists
psql -c "SELECT ProductCode FROM Products"

# Check license exists
psql -c "SELECT LicenseKey, Status FROM Licenses"

# Check device count
psql -c "SELECT COUNT(*) FROM LicenseDevices WHERE LicenseId = '...'"

# Verify ProductId matches
psql -c "SELECT p.ProductCode FROM Licenses l JOIN Products p ON l.ProductId = p.ProductId WHERE l.LicenseKey = '...'"
```

### 6. Signature Verification Fails

**Symptoms:**
- "INVALID_SIGNATURE"
- License validation fails
- Response not trusted

**Solutions:**
```bash
# Verify public key matches
# Check admin panel for current public key

# Check signing key version
psql -c "SELECT KeyVersion, IsActive FROM SigningKeys"

# Generate new key pair
# Use ISigningKeyService.GenerateSigningKeyPair()

# Verify canonical serialization
# Ensure JSON field ordering matches
```

### 7. Rate Limiting Issues

**Symptoms:**
- 429 Too Many Requests
- "RATE_LIMITED" error
- Requests being blocked

**Solutions:**
```bash
# Check rate limit config
cat appsettings.json | grep RateLimiting

# Adjust limits for development
# Set RateLimiting__LoginLimit to higher value

# Clear rate limit cache
# Restart API or wait for window to expire
```

### 8. Build Errors

**Symptoms:**
- Compilation errors
- Missing references
- Package restore failures

**Solutions:**
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build

# Clear NuGet cache
dotnet nuget locals all --clear

# Check package versions
dotnet list package

# Verify project references
dotnet list reference
```

### 9. Admin Panel Issues

**Symptoms:**
- Blank page
- API connection errors
- Build failures

**Solutions:**
```bash
# Clear node_modules
rm -rf node_modules
npm install

# Check API proxy
cat vite.config.ts

# Verify API URL
cat src/lib/api.ts

# Build for production
npm run build
```

### 10. Offline Validation Issues

**Symptoms:**
- "OFFLINE_GRACE_EXPIRED"
- "CLOCK_ROLLBACK_DETECTED"
- License invalid when offline

**Solutions:**
```bash
# Check offline grace hours
psql -c "SELECT OfflineGraceHours FROM LicensePlans"

# Verify local cache
# Check %APPDATA%\{ProductId}\license.cache

# Check system clock
w32tm /query /status

# Reset clock if needed
w32tm /resync
```

## Error Codes Reference

| Code | Meaning | Common Cause | Solution |
|------|---------|--------------|----------|
| `INVALID_LICENSE` | Key format invalid | Typo in key | Verify key format |
| `LICENSE_NOT_FOUND` | Key not in database | Wrong key | Check database |
| `LICENSE_EXPIRED` | Past expiry date | Time passed | Extend license |
| `LICENSE_REVoked` | Permanently revoked | Admin action | Generate new license |
| `LICENSE_SUSPENDED` | Temporarily disabled | Admin action | Reactivate |
| `PRODUCT_MISMATCH` | Wrong product | Wrong key/exe | Check product match |
| `DEVICE_LIMIT_REACHED` | Too many devices | Limit hit | Reset devices |
| `DEVICE_NOT_AUTHORIZED` | Unknown device | New device | Register device |
| `INVALID_SIGNATURE` | Signature failed | Tampering/key mismatch | Verify key |
| `SERVER_UNAVAILABLE` | Server unreachable | Network issue | Check connection |
| `OFFLINE_GRACE_EXPIRED` | Offline too long | No internet | Connect to validate |
| `CLOCK_ROLLBACK_DETECTED` | Clock manipulated | Time change | Correct clock |
| `RATE_LIMITED` | Too many requests | Rate limit | Wait or increase |
| `INVALID_REQUEST` | Bad request | Invalid input | Check request |
| `UNAUTHORIZED` | Not authenticated | No token | Login first |
| `FORBIDDEN` | Insufficient perms | Wrong role | Check permissions |

## Performance Issues

### Slow Activation
- Check database indexes
- Verify network latency
- Check server resources
- Review query performance

### Slow Validation
- Increase validation interval
- Check database performance
- Review caching strategy
- Monitor server load

### Memory Issues
- Check for memory leaks
- Review database connections
- Monitor API memory usage
- Check for large result sets

## Logging

### Enable Debug Logging

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "LicensePlatform": "Debug"
    }
  }
}
```

### View Logs

```bash
# Docker
docker logs -f license-api

# Systemd
journalctl -u license-api -f

# File logs
tail -f /var/log/license-platform/api.log
```

## Getting Help

### Check Logs First
1. API logs
2. Database logs
3. Client SDK logs

### Collect Information
- Error message
- Request ID
- Timestamp
- User/Device info
- API version

### Contact Support
- Include all collected information
- Steps to reproduce
- Expected vs actual behavior
