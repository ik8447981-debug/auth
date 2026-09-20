using LicensePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class ApiClientConfiguration : IEntityTypeConfiguration<ApiClient>
    {
        public void Configure(EntityTypeBuilder<ApiClient> builder)
        {
            builder.ToTable("ApiClients");

            builder.HasKey(c => c.ApiClientId);

            builder.Property(c => c.ApiClientId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(c => c.ClientName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.ApiKey)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.ApiSecret)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.Permissions)
                .HasMaxLength(1000)
                .HasDefaultValue(string.Empty);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(c => c.ApiKey)
                .IsUnique();
        }
    }
}
