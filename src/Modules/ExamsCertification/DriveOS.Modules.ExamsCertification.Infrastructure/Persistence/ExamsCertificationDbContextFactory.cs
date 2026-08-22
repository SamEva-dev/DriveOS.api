using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence;

internal sealed class ExamsCertificationDbContextFactory
    : IDesignTimeDbContextFactory<ExamsCertificationDbContext>
{
    public ExamsCertificationDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("DRIVEOS_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=DriveOS;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ExamsCertificationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable("__ef_migrations_history", ExamsCertificationSchema.Name));

        return new ExamsCertificationDbContext(optionsBuilder.Options);
    }
}
