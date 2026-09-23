using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LicensePlatform.Infrastructure.Data;

/// <summary>
/// Supplies a design-time DbContext for EF Core tooling. Runtime configuration is supplied by the API.
/// </summary>
public sealed class LicensePlatformDbContextFactory : IDesignTimeDbContextFactory<LicensePlatformDbContext>
{
    public LicensePlatformDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LicensePlatformDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=license_platform;Username=postgres;Password=postgres")
            .Options;

        return new LicensePlatformDbContext(options);
    }
}
