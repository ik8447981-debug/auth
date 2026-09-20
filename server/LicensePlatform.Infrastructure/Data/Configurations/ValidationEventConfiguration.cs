using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class ValidationEventConfiguration : IEntityTypeConfiguration<ValidationEvent>
    {
        public void Configure(EntityTypeBuilder<ValidationEvent> builder)
        {
            builder.ToTable("ValidationEvents");

            builder.HasKey(e => e.ValidationEventId);

            builder.Property(e => e.ValidationEventId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(e => e.LicenseId)
                .IsRequired();

            builder.Property(e => e.ProductId)
                .IsRequired();

            builder.Property(e => e.EventType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(e => e.DeviceFingerprint)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasDefaultValue(string.Empty);

            builder.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasDefaultValue(string.Empty);

            builder.Property(e => e.Success)
                .IsRequired();

            builder.Property(e => e.ErrorCode)
                .HasMaxLength(50);

            builder.HasIndex(e => e.LicenseId);

            builder.HasIndex(e => e.ServerTimestamp);

            builder.HasIndex(e => e.EventType);

            builder.HasOne(e => e.License)
                .WithMany(l => l.ValidationEvents)
                .HasForeignKey(e => e.LicenseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
