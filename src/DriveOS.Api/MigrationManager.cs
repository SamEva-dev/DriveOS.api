using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.Modules.CRM.Infrastructure.Persistence;
using Serilog;
using Microsoft.EntityFrameworkCore;

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
