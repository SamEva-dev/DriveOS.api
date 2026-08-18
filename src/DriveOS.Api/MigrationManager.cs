using DriveOS.Modules.CRM.Infrastructure.Persistence;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.Modules.Contracts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Itech.Emailing.Persistence;
using Serilog;

namespace DriveOS.Api
{
    public static class MigrationManager
    {
        public static IHost ApplyMigrations(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            try
            {
                // Apply OrganizationsDbContext migrations
                var orgDb = scope.ServiceProvider.GetRequiredService<OrganizationsDbContext>();
                Log.Information("Applying Organizations database migrations...");
                orgDb.Database.Migrate();
                Log.Information("✅ Organizations database migrated successfully.");

                var crmDb = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
                Log.Information("Applying CRM database migrations...");
                crmDb.Database.Migrate();
                Log.Information("✅ CRM database migrated successfully.");

                var studentsDb = scope.ServiceProvider.GetRequiredService<StudentsDbContext>();
                Log.Information("Applying Students database migrations...");
                studentsDb.Database.Migrate();
                Log.Information("✅ Students database migrated successfully.");

                var contractsDb = scope.ServiceProvider.GetRequiredService<ContractsDbContext>();
                Log.Information("Applying Contracts database migrations...");
                contractsDb.Database.Migrate();
                Log.Information("✅ Contracts database migrated successfully.");

                var fundingBillingDb = scope.ServiceProvider.GetRequiredService<FundingBillingDbContext>();
                Log.Information("Applying Funding & Billing database migrations...");
                fundingBillingDb.Database.Migrate();
                Log.Information("✅ Funding & Billing database migrated successfully.");

                var emailingDb = scope.ServiceProvider.GetRequiredService<EmailingDbContext>();
                Log.Information("Applying Emailing database migrations...");
                emailingDb.Database.Migrate();
                Log.Information("✅ Emailing database migrated successfully.");

                // Apply AuditDbContext migrations
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Migration failed.");
                throw;
            }

            return host;
        }
    }
}
