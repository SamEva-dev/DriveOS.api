using DriveOS.Modules.Contracts.Infrastructure.Persistence;
using DriveOS.Modules.CRM.Infrastructure.Persistence;
using DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence;
using DriveOS.Modules.ExamsCertification.Infrastructure.Persistence;
using DriveOS.Modules.FleetResources.Infrastructure.Persistence;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using Itech.Emailing.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DriveOS.Api;

public static class MigrationManager
{
    public static IHost ApplyMigrations(this IHost host)
    {
        using var scope = host.Services.CreateScope();

        try
        {
            ApplyMigration<OrganizationsDbContext>(
                scope.ServiceProvider,
                "Organizations");

            ApplyMigration<CrmDbContext>(
                scope.ServiceProvider,
                "CRM");

            ApplyMigration<StudentsDbContext>(
                scope.ServiceProvider,
                "Students");

            ApplyMigration<ContractsDbContext>(
                scope.ServiceProvider,
                "Contracts");

            ApplyMigration<FundingBillingDbContext>(
                scope.ServiceProvider,
                "Funding & Billing");

            ApplyMigration<CurriculumPedagogyDbContext>(
                scope.ServiceProvider,
                "Curriculum & Pedagogy");

            ApplyMigration<SchedulingCapacityDbContext>(
                scope.ServiceProvider,
                "Scheduling & Capacity");

            ApplyMigration<TrainingDeliveryDbContext>(
                scope.ServiceProvider,
                "Training Delivery");

            ApplyMigration<FleetResourcesDbContext>(
                scope.ServiceProvider,
                "Fleet & Resources");

            ApplyMigration<ExamsCertificationDbContext>(
                scope.ServiceProvider,
                "Exams & Certification");

            ApplyMigration<EmailingDbContext>(
                scope.ServiceProvider,
                "Emailing");
        }
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "❌ Database migration process failed.");

            throw;
        }

        return host;
    }

    private static void ApplyMigration<TDbContext>(
        IServiceProvider serviceProvider,
        string moduleName)
        where TDbContext : DbContext
    {
        Log.Information(
            "Applying {ModuleName} database migrations...",
            moduleName);

        var dbContext =
            serviceProvider.GetRequiredService<TDbContext>();

        var pendingMigrations =
            dbContext.Database
                .GetPendingMigrations()
                .ToArray();

        if (pendingMigrations.Length == 0)
        {
            Log.Information(
                "✅ {ModuleName}: database is already up to date.",
                moduleName);

            return;
        }

        Log.Information(
            "{ModuleName}: {PendingMigrationCount} pending migration(s): {PendingMigrations}",
            moduleName,
            pendingMigrations.Length,
            string.Join(", ", pendingMigrations));

        dbContext.Database.Migrate();

        Log.Information(
            "✅ {ModuleName} database migrated successfully.",
            moduleName);
    }
}