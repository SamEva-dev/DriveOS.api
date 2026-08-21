using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;

internal sealed class TrainingDeliveryDbContextFactory
    : IDesignTimeDbContextFactory<TrainingDeliveryDbContext>
{
    public TrainingDeliveryDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DriveOS")
            ?? throw new InvalidOperationException(
                "The environment variable 'ConnectionStrings__DriveOS' is missing.");

        var optionsBuilder = new DbContextOptionsBuilder<TrainingDeliveryDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            options => options.MigrationsHistoryTable(
                "__ef_migrations_history",
                TrainingDeliverySchema.Name));

        return new TrainingDeliveryDbContext(optionsBuilder.Options);
    }
}
