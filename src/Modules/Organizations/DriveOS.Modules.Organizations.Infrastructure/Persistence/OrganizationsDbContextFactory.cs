using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

internal sealed class OrganizationsDbContextFactory
    : IDesignTimeDbContextFactory<OrganizationsDbContext>
{
    public OrganizationsDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__DriveOS")
            ?? throw new InvalidOperationException(
                "The environment variable " +
                "'ConnectionStrings__DriveOS' is missing.");

        var optionsBuilder =
            new DbContextOptionsBuilder<OrganizationsDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            options =>
            {
                options.MigrationsHistoryTable(
                    "__ef_migrations_history",
                    OrganizationsSchema.Name);
            });

        return new OrganizationsDbContext(
            optionsBuilder.Options);
    }
}