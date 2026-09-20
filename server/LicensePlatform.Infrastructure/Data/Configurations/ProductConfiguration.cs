using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.ProductId);

            builder.Property(p => p.ProductId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(p => p.ProductName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.DisplayName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000)
                .HasDefaultValue(string.Empty);

            builder.Property(p => p.ProductCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(ProductStatus.Active);

            builder.Property(p => p.CurrentVersion)
                .HasMaxLength(50)
                .HasDefaultValue(string.Empty);

            builder.Property(p => p.MinimumSupportedVersion)
                .HasMaxLength(50)
                .HasDefaultValue(string.Empty);

            builder.Property(p => p.LatestVersion)
                .HasMaxLength(50)
                .HasDefaultValue(string.Empty);

            builder.Property(p => p.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(p => p.ProductCode)
                .IsUnique();

            builder.HasIndex(p => p.ProductName)
                .IsUnique();

            // Query filter for soft delete
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
