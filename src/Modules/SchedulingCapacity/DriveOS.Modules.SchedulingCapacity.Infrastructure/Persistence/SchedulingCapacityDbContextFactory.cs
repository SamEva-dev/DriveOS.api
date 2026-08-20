using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;

internal sealed class SchedulingCapacityDbContextFactory
    : IDesignTimeDbContextFactory<SchedulingCapacityDbContext>
{
    public SchedulingCapacityDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DriveOS")
            ?? throw new InvalidOperationException(
                "The environment variable 'ConnectionStrings__DriveOS' is missing.");

        var optionsBuilder = new DbContextOptionsBuilder<SchedulingCapacityDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            options => options.MigrationsHistoryTable(
                "__ef_migrations_history",
                SchedulingCapacitySchema.Name));

        return new SchedulingCapacityDbContext(optionsBuilder.Options);
    }
}
