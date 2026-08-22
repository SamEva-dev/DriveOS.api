using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DriveOS.Modules.FleetResources.Infrastructure.Persistence;

internal sealed class FleetResourcesDbContextFactory : IDesignTimeDbContextFactory<FleetResourcesDbContext>
{
    public FleetResourcesDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("DRIVEOS_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=DriveOS;Username=postgres;Password=postgres";
        var options = new DbContextOptionsBuilder<FleetResourcesDbContext>();
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable("__ef_migrations_history", FleetResourcesSchema.Name));
        return new FleetResourcesDbContext(options.Options);
    }
}
