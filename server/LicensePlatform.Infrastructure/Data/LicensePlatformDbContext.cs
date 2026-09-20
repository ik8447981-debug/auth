using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Infrastructure.Data
{
    /// <summary>
    /// Primary database context for the License Platform.
    /// Configures all entity mappings, relationships, constraints, and indexes.
    /// </summary>
    public class LicensePlatformDbContext : DbContext
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductFeature> ProductFeatures => Set<ProductFeature>();
        public DbSet<ProductVersion> ProductVersions => Set<ProductVersion>();
        public DbSet<LicensePlan> LicensePlans => Set<LicensePlan>();
        public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<License> Licenses => Set<License>();
        public DbSet<LicenseFeature> LicenseFeatures => Set<LicenseFeature>();
        public DbSet<LicenseDevice> LicenseDevices => Set<LicenseDevice>();
        public DbSet<ValidationEvent> ValidationEvents => Set<ValidationEvent>();
        public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<SigningKey> SigningKeys => Set<SigningKey>();
        public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
        public DbSet<ApiClient> ApiClients => Set<ApiClient>();

        public LicensePlatformDbContext(DbContextOptions<LicensePlatformDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations from the Data/Configurations folder
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LicensePlatformDbContext).Assembly);

            // Configure PostgreSQL-specific defaults
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Set default CreatedAt for all entities that have it
                var createdAtProp = entityType.FindProperty("CreatedAt");
                if (createdAtProp != null && createdAtProp.ClrType == typeof(DateTime))
                {
                    createdAtProp.SetDefaultValueSql("NOW()");
                }

                // Set default UpdatedAt for all entities that have it
                var updatedAtProp = entityType.FindProperty("UpdatedAt");
                if (updatedAtProp != null && updatedAtProp.ClrType == typeof(DateTime))
                {
                    updatedAtProp.SetDefaultValueSql("NOW()");
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    // Auto-fill CreatedAt
                    var createdAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                    if (createdAtProp != null && createdAtProp.CurrentValue == null)
                    {
                        createdAtProp.CurrentValue = utcNow;
                    }

                    // Auto-fill UpdatedAt on insert
                    var updatedAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                    if (updatedAtProp != null)
                    {
                        updatedAtProp.CurrentValue = utcNow;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Auto-fill UpdatedAt on update
                    var updatedAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                    if (updatedAtProp != null)
                    {
                        updatedAtProp.CurrentValue = utcNow;
                        // Prevent UpdatedAt from being modified by caller
                        entry.Property("UpdatedAt").IsModified = true;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
