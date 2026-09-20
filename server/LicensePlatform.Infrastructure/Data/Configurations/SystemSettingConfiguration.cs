using LicensePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
    {
        public void Configure(EntityTypeBuilder<SystemSetting> builder)
        {
            builder.ToTable("SystemSettings");

            builder.HasKey(s => s.SystemSettingId);

            builder.Property(s => s.SystemSettingId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(s => s.SettingKey)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.SettingValue)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(s => s.Description)
                .HasMaxLength(1000)
                .HasDefaultValue(string.Empty);

            builder.HasIndex(s => s.SettingKey)
                .IsUnique();
        }
    }
}
