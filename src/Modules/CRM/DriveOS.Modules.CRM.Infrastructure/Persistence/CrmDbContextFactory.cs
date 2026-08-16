using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence;

internal sealed class CrmDbContextFactory : IDesignTimeDbContextFactory<CrmDbContext>
{
    public CrmDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DriveOS")
            ?? throw new InvalidOperationException(
                "The environment variable " + "'ConnectionStrings__DriveOS' is missing."
            );

        var optionsBuilder = new DbContextOptionsBuilder<CrmDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            options =>
            {
                options.MigrationsHistoryTable("__ef_migrations_history", CrmSchema.Name);
            }
        );

        return new CrmDbContext(optionsBuilder.Options);
    }
}
