using LicensePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class SigningKeyConfiguration : IEntityTypeConfiguration<SigningKey>
    {
        public void Configure(EntityTypeBuilder<SigningKey> builder)
        {
            builder.ToTable("SigningKeys");

            builder.HasKey(k => k.SigningKeyId);

            builder.Property(k => k.SigningKeyId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(k => k.KeyVersion)
                .IsRequired();

            builder.Property(k => k.PublicKeyPem)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(k => k.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(k => k.KeyVersion)
                .IsUnique();

            builder.HasIndex(k => k.IsActive);
        }
    }
}
