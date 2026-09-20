# License Platform - Deployment Guide

## Prerequisites

- .NET 8 SDK
- PostgreSQL 14+
- Node.js 18+ (for admin panel)
- Docker (optional)

## Development Setup

### 1. Clone and Build

```bash
git clone <repository>
cd license-platform

# Build solution
dotnet restore server/LicensePlatform.sln
dotnet build server/LicensePlatform.sln
```

### 2. Database Setup

```bash
# Create PostgreSQL database
createdb license_platform

# Update connection string in appsettings.json
# "DefaultConnection": "Host=localhost;Database=license_platform;Username=postgres;Password=yourpassword"

# Run migrations
cd server/LicensePlatform.Api
dotnet ef database update
```

### 3. Start API Server

```bash
cd server/LicensePlatform.Api
dotnet run
# API available at http://localhost:5000
# Swagger at http://localhost:5000/swagger
```

### 4. Start Admin Panel

```bash
cd admin-panel
npm install
npm run dev
# Admin panel at http://localhost:5173
```

## Production Deployment

### Option 1: Docker

**docker-compose.yml:**
```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: server/LicensePlatform.Api/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=db;Database=license_platform;Username=postgres;Password=${DB_PASSWORD}
      - JwtSettings__SecretKey=${JWT_SECRET}
    depends_on:
      - db

  admin:
    build:
      context: .
      dockerfile: admin-panel/Dockerfile
    ports:
      - "80:80"
    depends_on:
      - api

  db:
    image: postgres:15-alpine
    environment:
      - POSTGRES_DB=license_platform
      - POSTGRES_PASSWORD=${DB_PASSWORD}
    volumes:
      - pgdata:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  nginx:
    image: nginx:alpine
    ports:
      - "443:443"
      - "80:80"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./certs:/etc/nginx/certs
    depends_on:
      - api
      - admin

volumes:
  pgdata:
```

### Option 2: IIS / Windows Server

```bash
# Publish API
dotnet publish server/LicensePlatform.Api -c Release -o ./publish

# Copy to IIS
# Configure application pool: No Managed Code
# Set environment variables in web.config
```

### Option 3: Linux ( systemd)

```bash
# Publish
dotnet publish server/LicensePlatform.Api -c Release -o /opt/license-api

# Create service file
sudo nano /etc/systemd/system/license-api.service

# Enable and start
sudo systemctl enable license-api
sudo systemctl start license-api
```

## Environment Variables

### Required
```
ConnectionStrings__DefaultConnection=Host=localhost;Database=license_platform;Username=postgres;Password=secret
JwtSettings__SecretKey=your-256-bit-secret-key-here
```

### Optional
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
RateLimiting__LoginLimit=5
RateLimiting__ActivationLimit=10
CorsOrigins__0=https://admin.yourdomain.com
```

## SSL/TLS Configuration

### Let's Encrypt (Free)

```bash
# Install certbot
sudo apt install certbot

# Get certificate
sudo certbot certonly --standalone -d license.yourdomain.com

# Certificates at:
# /etc/letsencrypt/live/license.yourdomain.com/fullchain.pem
# /etc/letsencrypt/live/license.yourdomain.com/privkey.pem
```

### Nginx Reverse Proxy

```nginx
server {
    listen 80;
    server_name license.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl;
    server_name license.yourdomain.com;

    ssl_certificate /etc/nginx/certs/fullchain.pem;
    ssl_certificate_key /etc/nginx/certs/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;

    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /admin {
        alias /var/www/admin-panel/dist;
        try_files $uri $uri/ /admin/index.html;
    }
}
```

## Database Backup

### Automated Backup (PostgreSQL)

```bash
#!/bin/bash
# backup.sh

BACKUP_DIR="/backups"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="license_platform"

# Create backup
pg_dump -U postgres $DB_NAME | gzip > $BACKUP_DIR/$DB_NAME_$DATE.sql.gz

# Keep only last 30 days
find $BACKUP_DIR -name "*.sql.gz" -mtime +30 -delete
```

### Cron Job

```bash
# Daily backup at 2 AM
0 2 * * * /opt/license-platform/backup.sh
```

## Monitoring

### Health Check

```bash
curl http://localhost:5000/health
# Returns: {"status": "healthy"}
```

### Log Monitoring

```bash
# View API logs
docker logs -f license-api

# Or for systemd
journalctl -u license-api -f
```

### Key Metrics to Monitor
- API response times
- Database connection pool
- License activation rates
- Validation success/failure rates
- Rate limit violations
- Error rates

## Security Checklist

- [ ] HTTPS enabled
- [ ] Strong JWT secret (256+ bits)
- [ ] Strong database password
- [ ] Firewall configured (only ports 80, 443)
- [ ] Rate limiting enabled
- [ ] Audit logging enabled
- [ ] Database backups scheduled
- [ ] Secrets in environment variables (not in code)
- [ ] CORS properly configured
- [ ] Security headers enabled
- [ ] Regular dependency updates
- [ ] Vulnerability scanning

## Troubleshooting

### Database Connection Issues
```bash
# Test connection
psql -h localhost -U postgres -d license_platform

# Check PostgreSQL status
sudo systemctl status postgresql
```

### Migration Issues
```bash
# Check pending migrations
dotnet ef migrations list

# Apply specific migration
dotnet ef database update <MigrationName>
```

### API Not Starting
```bash
# Check logs
dotnet run --verbosity detailed

# Verify environment variables
echo $ConnectionStrings__DefaultConnection
```

### Admin Panel Build Issues
```bash
# Clear cache
rm -rf node_modules
npm install

# Check for TypeScript errors
npm run build
```
