using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
    {
        public void Configure(EntityTypeBuilder<AdminUser> builder)
        {
            builder.ToTable("AdminUsers");

            builder.HasKey(a => a.AdminUserId);

            builder.Property(a => a.AdminUserId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(a => a.Username)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(a => a.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(a => a.Username)
                .IsUnique();

            builder.HasIndex(a => a.Email)
                .IsUnique();
        }
    }
}
