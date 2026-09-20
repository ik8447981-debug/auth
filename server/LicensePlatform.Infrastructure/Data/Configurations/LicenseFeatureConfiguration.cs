using LicensePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class LicenseFeatureConfiguration : IEntityTypeConfiguration<LicenseFeature>
    {
        public void Configure(EntityTypeBuilder<LicenseFeature> builder)
        {
            builder.ToTable("LicenseFeatures");

            builder.HasKey(lf => lf.LicenseFeatureId);

            builder.Property(lf => lf.LicenseFeatureId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(lf => lf.LicenseId)
                .IsRequired();

            builder.Property(lf => lf.FeatureId)
                .IsRequired();

            builder.HasIndex(lf => new { lf.LicenseId, lf.FeatureId })
                .IsUnique();

            builder.HasOne(lf => lf.License)
                .WithMany(l => l.LicenseFeatures)
                .HasForeignKey(lf => lf.LicenseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(lf => lf.Feature)
                .WithMany(f => f.LicenseFeatures)
                .HasForeignKey(lf => lf.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
