using LicensePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
    {
        public void Configure(EntityTypeBuilder<PlanFeature> builder)
        {
            builder.ToTable("PlanFeatures");

            builder.HasKey(pf => pf.Id);

            builder.Property(pf => pf.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(pf => pf.PlanId)
                .IsRequired();

            builder.Property(pf => pf.FeatureId)
                .IsRequired();

            builder.HasIndex(pf => new { pf.PlanId, pf.FeatureId })
                .IsUnique();

            builder.HasOne(pf => pf.Plan)
                .WithMany(p => p.PlanFeatures)
                .HasForeignKey(pf => pf.PlanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pf => pf.Feature)
                .WithMany(f => f.PlanFeatures)
                .HasForeignKey(pf => pf.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
