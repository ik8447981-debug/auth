using LicensePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class LicensePlanConfiguration : IEntityTypeConfiguration<LicensePlan>
    {
        public void Configure(EntityTypeBuilder<LicensePlan> builder)
        {
            builder.ToTable("LicensePlans");

            builder.HasKey(p => p.PlanId);

            builder.Property(p => p.PlanId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(p => p.ProductId)
                .IsRequired();

            builder.Property(p => p.PlanName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.DurationDays)
                .IsRequired()
                .HasDefaultValue(30);

            builder.Property(p => p.MaxDevices)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(p => p.OfflineGraceHours)
                .HasDefaultValue(72);

            builder.Property(p => p.ValidationIntervalMinutes)
                .HasDefaultValue(24);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(p => new { p.ProductId, p.PlanName })
                .IsUnique();

            builder.HasOne(p => p.Product)
                .WithMany(p => p.Plans)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
