using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Conversions;
using DriveOS.Modules.CRM.Domain.Tasks;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.Modules.CRM.Infrastructure.Persistence;
using DriveOS.Modules.CRM.Infrastructure.Persistence.Repositories;
using DriveOS.Modules.CRM.Infrastructure.Persistence.Interceptors;
using DriveOS.Modules.CRM.Infrastructure.Persistence.Queries;
using DriveOS.Modules.CRM.Application.Dashboard.GetDashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.Modules.CRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCrmInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString =
            configuration.GetConnectionString("DriveOS")
            ?? throw new InvalidOperationException(
                "The DriveOS database connection string is missing.");

        services.AddScoped<CrmAuditableEntityInterceptor>();

        services.AddDbContext<CrmDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable(
                        "__ef_migrations_history",
                        CrmSchema.Name);
                });

            options.AddInterceptors(
                serviceProvider.GetRequiredService<CrmAuditableEntityInterceptor>());
        });

        // Never register CRM as the global IUnitOfWork: Organizations already
        // owns that registration. Each module must resolve its own UoW port.
        services.AddScoped<ICrmUnitOfWork>(
            serviceProvider =>
                serviceProvider.GetRequiredService<CrmDbContext>());

        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<ILeadConversionRepository, LeadConversionRepository>();
        services.AddScoped<ILeadReadService, LeadReadService>();
        services.AddScoped<ICrmTaskRepository, CrmTaskRepository>();
        services.AddScoped<ICrmActivityRepository, CrmActivityRepository>();
        services.AddScoped<IAssessmentAppointmentRepository, AssessmentAppointmentRepository>();
        services.AddScoped<IAssessmentSessionRepository, AssessmentSessionRepository>();
        services.AddScoped<ICommercialOfferRepository, CommercialOfferRepository>();
        services.AddScoped<ICrmDashboardReadService, CrmDashboardReadService>();

        return services;
    }
}
