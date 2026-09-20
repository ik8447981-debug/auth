using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(a => a.AuditLogId);

            builder.Property(a => a.AuditLogId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(a => a.ActorName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.Action)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(a => a.TargetType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.TargetId)
                .HasMaxLength(100)
                .HasDefaultValue(string.Empty);

            builder.Property(a => a.IpAddress)
                .HasMaxLength(45)
                .HasDefaultValue(string.Empty);

            builder.Property(a => a.Result)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Metadata)
                .HasMaxLength(4000);

            builder.HasIndex(a => a.ActorName);

            builder.HasIndex(a => a.Timestamp);

            builder.HasIndex(a => new { a.TargetType, a.TargetId });

            builder.HasIndex(a => a.Action);
        }
    }
}
