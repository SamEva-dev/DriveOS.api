using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Application.Auditing;
using DriveOS.Modules.Contracts.Application.ContractDocuments;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Generate;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Read;
using DriveOS.Modules.Contracts.Domain.SignatureProcesses;
using DriveOS.Modules.Contracts.Domain.ContractDocuments;
using DriveOS.Modules.Contracts.Domain.ContractAmendments;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.Modules.Contracts.Domain.ProfessionalServiceContracts;
using DriveOS.Modules.Contracts.Infrastructure.Documents;
using DriveOS.Modules.Contracts.Infrastructure.Persistence;
using DriveOS.Modules.Contracts.Infrastructure.Persistence.Repositories;
using DriveOS.Modules.Contracts.Infrastructure.Read;
using DriveOS.Modules.Contracts.Infrastructure.Auditing;
using DriveOS.Modules.Contracts.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.Modules.Contracts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddContractsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DriveOS")
            ?? throw new InvalidOperationException("The DriveOS database connection string is missing.");

        services.AddScoped<ContractsAuditInterceptor>();
        services.AddDbContext<ContractsDbContext>((provider, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", ContractsSchema.Name));
            options.AddInterceptors(provider.GetRequiredService<ContractsAuditInterceptor>());
        });

        services.AddScoped<IContractsUnitOfWork>(sp => sp.GetRequiredService<ContractsDbContext>());
        services.AddScoped<ITrainingContractRepository, TrainingContractRepository>();
        services.AddScoped<IProfessionalServiceContractRepository, ProfessionalServiceContractRepository>();
        services.AddScoped<IContractAmendmentRepository, ContractAmendmentRepository>();
        services.AddScoped<ISignatureProcessRepository, SignatureProcessRepository>();
        services.AddScoped<IContractDocumentRepository, ContractDocumentRepository>();
        services.AddScoped<ITrainingContractReadService, TrainingContractReadService>();
        services.AddScoped<IContractDocumentReadService, ContractDocumentReadService>();
        services.AddScoped<IContractAuditReadService, ContractAuditReadService>();
        services.AddScoped<ITrainingContractDocumentGenerator, TrainingContractDocumentGenerator>();
        services.AddSingleton<ITrainingContractDocumentStorage, EncryptedTrainingContractDocumentStorage>();
        return services;
    }
}
