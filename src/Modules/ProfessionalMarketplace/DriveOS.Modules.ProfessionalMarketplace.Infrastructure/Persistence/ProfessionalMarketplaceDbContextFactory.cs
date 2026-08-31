using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence;

public sealed class ProfessionalMarketplaceDbContextFactory : IDesignTimeDbContextFactory<ProfessionalMarketplaceDbContext>
{
    public ProfessionalMarketplaceDbContext CreateDbContext(string[] args)
    {
        string cs = Environment.GetEnvironmentVariable("ConnectionStrings__DriveOS")
            ?? "Host=localhost;Port=5432;Database=driveos;Username=postgres;Password=postgres";
        var options = new DbContextOptionsBuilder<ProfessionalMarketplaceDbContext>()
            .UseNpgsql(cs, x => x.MigrationsHistoryTable("__ef_migrations_history", ProfessionalMarketplaceSchema.Name))
            .Options;
        return new ProfessionalMarketplaceDbContext(options);
    }
}
