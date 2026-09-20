using LicensePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class ProductFeatureConfiguration : IEntityTypeConfiguration<ProductFeature>
    {
        public void Configure(EntityTypeBuilder<ProductFeature> builder)
        {
            builder.ToTable("ProductFeatures");

            builder.HasKey(f => f.FeatureId);

            builder.Property(f => f.FeatureId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(f => f.ProductId)
                .IsRequired();

            builder.Property(f => f.FeatureKey)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.FeatureName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(f => f.Description)
                .HasMaxLength(1000)
                .HasDefaultValue(string.Empty);

            builder.Property(f => f.IsEnabled)
                .HasDefaultValue(true);

            builder.HasIndex(f => new { f.ProductId, f.FeatureKey })
                .IsUnique();

            builder.HasOne(f => f.Product)
                .WithMany(p => p.Features)
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
