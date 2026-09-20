#!/bin/bash
# License Platform - Database Backup Script

set -e

# Configuration
BACKUP_DIR="/backups/license-platform"
DB_NAME="license_platform"
DB_USER="postgres"
DB_HOST="localhost"
RETENTION_DAYS=30
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/$DB_NAME_$DATE.sql.gz"

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Create backup
echo "Creating backup: $BACKUP_FILE"
pg_dump -h "$DB_HOST" -U "$DB_USER" "$DB_NAME" | gzip > "$BACKUP_FILE"

# Verify backup
if [ -f "$BACKUP_FILE" ] && [ -s "$BACKUP_FILE" ]; then
    echo "Backup successful: $BACKUP_FILE"
    echo "Size: $(du -h "$BACKUP_FILE" | cut -f1)"
else
    echo "ERROR: Backup failed"
    exit 1
fi

# Cleanup old backups
echo "Cleaning up backups older than $RETENTION_DAYS days..."
find "$BACKUP_DIR" -name "*.sql.gz" -mtime +$RETENTION_DAYS -delete

# List remaining backups
echo "Current backups:"
ls -lh "$BACKUP_DIR"/*.sql.gz 2>/dev/null || echo "No backups found"

echo "Backup complete!"
