using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence;
public sealed class WorkforceDbContextFactory : IDesignTimeDbContextFactory<WorkforceDbContext>
{
    public WorkforceDbContext CreateDbContext(string[] args)
    {
        string cs = Environment.GetEnvironmentVariable("ConnectionStrings__DriveOS") ?? "Host=localhost;Port=5432;Database=driveos;Username=postgres;Password=postgres";
        var options = new DbContextOptionsBuilder<WorkforceDbContext>().UseNpgsql(cs, x => x.MigrationsHistoryTable("__ef_migrations_history", WorkforceSchema.Name)).Options;
        return new WorkforceDbContext(options);
    }
}
