using DriveOS.Modules.RegulatoryIntegrations.Application.Administration;
using DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Administration;
using DriveOS.Modules.RegulatoryIntegrations.Application.Persistence;
using DriveOS.Modules.RegulatoryIntegrations.Application.Submissions;
using DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;
using DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Persistence;
using DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Persistence.Repositories;
using DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Submissions;
using DriveOS.Modules.RegulatoryIntegrations.Application.Dispatching;
using DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Dispatching;
using DriveOS.Modules.RegulatoryIntegrations.Application.Reconciliation;
using DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Reconciliation;
using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddRegulatoryIntegrationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string cs = configuration.GetConnectionString("DriveOS") ?? throw new InvalidOperationException("The DriveOS database connection string is missing.");
        services.AddDbContext<RegulatoryIntegrationsDbContext>(o => o.UseNpgsql(cs, n => n.MigrationsHistoryTable("__ef_migrations_history", RegulatoryIntegrationsSchema.Name)));
        services.AddScoped<IRegulatoryIntegrationsUnitOfWork>(sp => sp.GetRequiredService<RegulatoryIntegrationsDbContext>());
        services.AddScoped<IRegulatoryTrainingRecordSubmissionRepository, RegulatoryTrainingRecordSubmissionRepository>();
        services.AddScoped<IRegulatoryTrainingRecordSubmissionService, RegulatoryTrainingRecordSubmissionService>();
        services.AddScoped<IRegulatoryTrainingRecordAdministrationService, RegulatoryTrainingRecordAdministrationService>();
        services.AddScoped<IRegulatoryTrainingRecordSubmissionDispatchStore, RegulatoryTrainingRecordSubmissionDispatchStore>();
        services.AddScoped<IRegulatoryTrainingRecordTransportDispatcher, RegulatoryTrainingRecordTransportDispatcher>();

        services.Configure<RegulatoryTrainingRecordDispatchOptions>(configuration.GetSection(RegulatoryTrainingRecordDispatchOptions.SectionName));
        services.AddHostedService<RegulatoryTrainingRecordSubmissionWorker>();
        services.AddScoped<IRegulatoryTrainingRecordReconciliationStore, RegulatoryTrainingRecordReconciliationStore>();
        services.Configure<RegulatoryTrainingRecordReconciliationOptions>(configuration.GetSection("RegulatoryIntegrations:TrainingRecordReconciliation"));
        services.AddHostedService<RegulatoryTrainingRecordReconciliationWorker>();
        return services;
    }
}
