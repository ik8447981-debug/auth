using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class LicenseConfiguration : IEntityTypeConfiguration<License>
    {
        public void Configure(EntityTypeBuilder<License> builder)
        {
            builder.ToTable("Licenses");

            builder.HasKey(l => l.LicenseId);

            builder.Property(l => l.LicenseId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(l => l.LicenseKey)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(l => l.ProductId)
                .IsRequired();

            builder.Property(l => l.PlanId)
                .IsRequired();

            builder.Property(l => l.LicenseType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(l => l.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(LicenseStatus.Created);

            builder.Property(l => l.MaxDevices)
                .HasDefaultValue(1);

            builder.Property(l => l.FailedValidationCount)
                .HasDefaultValue(0);

            builder.HasIndex(l => l.LicenseKey)
                .IsUnique();

            builder.HasIndex(l => l.ProductId);

            builder.HasIndex(l => l.CustomerId);

            builder.HasIndex(l => l.Status);

            builder.HasIndex(l => l.ExpiryDate);

            builder.HasOne(l => l.Product)
                .WithMany(p => p.Licenses)
                .HasForeignKey(l => l.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.Customer)
                .WithMany(c => c.Licenses)
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(l => l.Plan)
                .WithMany(p => p.Licenses)
                .HasForeignKey(l => l.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
