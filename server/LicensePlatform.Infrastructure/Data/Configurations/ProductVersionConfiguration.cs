using LicensePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class ProductVersionConfiguration : IEntityTypeConfiguration<ProductVersion>
    {
        public void Configure(EntityTypeBuilder<ProductVersion> builder)
        {
            builder.ToTable("ProductVersions");

            builder.HasKey(v => v.VersionId);

            builder.Property(v => v.VersionId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(v => v.ProductId)
                .IsRequired();

            builder.Property(v => v.VersionNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(v => v.IsLatest)
                .HasDefaultValue(false);

            builder.HasIndex(v => new { v.ProductId, v.VersionNumber })
                .IsUnique();

            builder.HasOne(v => v.Product)
                .WithMany()
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
