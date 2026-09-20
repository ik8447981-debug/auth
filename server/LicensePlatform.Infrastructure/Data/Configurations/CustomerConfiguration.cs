using LicensePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicensePlatform.Infrastructure.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");

            builder.HasKey(c => c.CustomerId);

            builder.Property(c => c.CustomerId)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(c => c.ProductId)
                .IsRequired();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(c => c.Notes)
                .HasMaxLength(2000)
                .HasDefaultValue(string.Empty);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            // Email must be unique per product
            builder.HasIndex(c => new { c.ProductId, c.Email })
                .IsUnique();

            builder.HasOne(c => c.Product)
                .WithMany(p => p.Customers)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
