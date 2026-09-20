using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class LicenseDeviceConfiguration : IEntityTypeConfiguration<LicenseDevice>
    {
        public void Configure(EntityTypeBuilder<LicenseDevice> builder)
        {
            builder.ToTable("LicenseDevices");

            builder.HasKey(d => d.LicenseDeviceId);

            builder.Property(d => d.LicenseDeviceId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(d => d.LicenseId)
                .IsRequired();

            builder.Property(d => d.DeviceFingerprint)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(d => d.DeviceName)
                .HasMaxLength(200)
                .HasDefaultValue(string.Empty);

            builder.Property(d => d.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(DeviceStatus.Active);

            builder.Property(d => d.ApplicationVersion)
                .HasMaxLength(50)
                .HasDefaultValue(string.Empty);

            builder.Property(d => d.OSVersion)
                .HasMaxLength(200)
                .HasDefaultValue(string.Empty);

            builder.HasIndex(d => d.DeviceFingerprint);

            builder.HasIndex(d => new { d.LicenseId, d.DeviceFingerprint })
                .IsUnique();

            builder.HasOne(d => d.License)
                .WithMany(l => l.Devices)
                .HasForeignKey(d => d.LicenseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
