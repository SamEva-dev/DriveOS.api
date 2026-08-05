using DriveOS.Modules.Organizations.Infrastructure.OrganizationSequences;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application.BranchAssignments;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides;
using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Organizations.Application.Branches.Managers;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Effective;
using DriveOS.Modules.Organizations.Application.OrganizationSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions;
using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.Modules.Organizations.Infrastructure.Authentication;
using DriveOS.Modules.Organizations.Infrastructure.BranchConfigurationOverrides;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationConfigurations;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.Modules.Organizations.Infrastructure.Persistence.Interceptors;
using DriveOS.Modules.Organizations.Infrastructure.Persistence.Queries;
using DriveOS.Modules.Organizations.Infrastructure.Persistence.Repositories;
using DriveOS.Modules.Organizations.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DriveOS.Modules.Organizations.Application.OrganizationSequences;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationRepresentatives.AccessSynchronization;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationRepresentatives.Expiration;
using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationLegalProfiles;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationActivationReadiness;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Expiration;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationLegalProfiles.Compliance;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Rules;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationActivationReadiness.Audit;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Audit;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Readiness;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Commands;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationClosures;

namespace DriveOS.Modules.Organizations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString =
            configuration.GetConnectionString("DriveOS")
            ?? throw new InvalidOperationException(
                "The DriveOS database connection string is missing.");

        services.AddSingleton<IClock, SystemClock>();

        services.TryAddScoped<ICurrentUser, AnonymousCurrentUser>();

        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<OrganizationsDbContext>(
            (serviceProvider, options) =>
            {
                var auditInterceptor =
                    serviceProvider.GetRequiredService<
                        AuditableEntityInterceptor>();

                options.UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsHistoryTable(
                            "__ef_migrations_history",
                            OrganizationsSchema.Name);
                    });

                options.AddInterceptors(auditInterceptor);
            });

        services.AddScoped<
            IOrganizationRepository,
            OrganizationRepository>();

        services.AddScoped<
            IOrganizationReadService,
            OrganizationReadService>();

        services.AddScoped<
            IOrganizationSubscriptionRepository,
            OrganizationSubscriptionRepository>();

        services.AddScoped<
            IOrganizationSubscriptionReadService,
            OrganizationSubscriptionReadService>();
        services.AddScoped<IOrganizationSettingsRepository, OrganizationSettingsRepository>();

        services.AddScoped<
                IOrganizationConfigurationRepository,
                OrganizationConfigurationRepository>();

        services.AddScoped<
            IOrganizationSequenceRepository,
            OrganizationSequenceRepository>();

        services.AddScoped<
            IOrganizationSequenceReadService,
            OrganizationSequenceReadService>();

        services.Configure<OrganizationSequenceReservationOptions>(options =>
            configuration
                .GetSection(OrganizationSequenceReservationOptions.SectionName)
                .Bind(options));

        services.AddScoped<
            IOrganizationSequenceNumberGenerator,
            OrganizationSequenceNumberGenerator>();
        services.AddScoped<
            IBranchConfigurationOverrideRepository,
            BranchConfigurationOverrideRepository>();

        services.AddScoped<
    IOrganizationRepresentativeRepository,
    OrganizationRepresentativeRepository>();

    services.AddScoped<
    IOrganizationLegalProfileRepository,
    OrganizationLegalProfileRepository>();


        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IBranchReadService, BranchReadService>();

        services.AddScoped<IUnitOfWork>(
            serviceProvider =>
                serviceProvider.GetRequiredService<
                    OrganizationsDbContext>());

        services.AddScoped<IBranchManagerReadService, BranchManagerReadService>();
        services.AddScoped<IBranchUserAssignmentRepository, BranchUserAssignmentRepository>();
        services.AddScoped<IBranchUserAssignmentReadService, BranchUserAssignmentReadService>();
        services.AddScoped<IOrganizationSettingsReadService, OrganizationSettingsReadService>();
        services.AddScoped<IOrganizationConfigurationReadService, OrganizationConfigurationReadService>();

        services.AddSingleton<OrganizationConfigurationMemoryCache>();

        services.AddSingleton<IOrganizationConfigurationCacheInvalidator>(
            serviceProvider =>
                serviceProvider.GetRequiredService<
                    OrganizationConfigurationMemoryCache>());

        services.AddScoped<
            IEffectiveOrganizationConfigurationResolver,
            EffectiveOrganizationConfigurationResolver>();

        services.AddScoped<
            IBranchConfigurationOverrideReadService,
            BranchConfigurationOverrideReadService>();

        services.Configure<BranchConfigurationOverridePolicyOptions>(options =>
            configuration
                .GetSection(BranchConfigurationOverridePolicyOptions.SectionName)
                .Bind(options));

        services.AddSingleton<IJsonConfigurationMerger, JsonConfigurationMerger>();
        services.AddSingleton<IBranchConfigurationMergePolicy, BranchConfigurationMergePolicy>();
        services.AddScoped<IOrganizationRepresentativeReadService, OrganizationRepresentativeReadService>();

        services.Configure<AuthGateRepresentativeAccessOptions>(options =>
            configuration
                .GetSection(AuthGateRepresentativeAccessOptions.SectionName)
                .Bind(options));

        services.Configure<OrganizationRepresentativeExpirationOptions>(options =>
            configuration
                .GetSection(OrganizationRepresentativeExpirationOptions.SectionName)
                .Bind(options));

        bool representativeAuthGateEnabled =
            configuration.GetValue<bool>("AuthGate:OrganizationRepresentatives:Enabled");
        string? authGateBaseUrl = configuration["AuthGate:BaseUrl"];

        if (representativeAuthGateEnabled && Uri.TryCreate(authGateBaseUrl, UriKind.Absolute, out Uri? authGateUri))
        {
            services.AddHttpClient<IOrganizationRepresentativeAccessSynchronizer,
                AuthGateOrganizationRepresentativeAccessSynchronizer>(client =>
                {
                    client.BaseAddress = authGateUri;
                });
        }
        else
        {
            services.AddScoped<IOrganizationRepresentativeAccessSynchronizer,
                NoOpOrganizationRepresentativeAccessSynchronizer>();
        }

        services.AddScoped<OrganizationRepresentativeAccessSynchronizationService>();
        services.AddScoped<IOrganizationRepresentativeExpirationProcessor,
            OrganizationRepresentativeExpirationProcessor>();
        services.AddHostedService<OrganizationRepresentativeExpirationWorker>();

        services.AddScoped<IOrganizationLegalProfileReadService, OrganizationLegalProfileReadService>();
        services.AddScoped<IOrganizationLegalProfileCountryRules, GenericOrganizationLegalProfileCountryRules>();
        services.AddScoped<IOrganizationLegalProfileCountryRules, FranceOrganizationLegalProfileCountryRules>();
        services.AddScoped<IOrganizationLegalProfileCountryRulesProvider, OrganizationLegalProfileCountryRulesProvider>();
        services.AddScoped<IOrganizationLegalProfileComplianceService, OrganizationLegalProfileComplianceService>();

        services.AddScoped<
            IOrganizationActivationReadinessDataSource,
            OrganizationActivationReadinessDataSource>();

            services.AddScoped<IOrganizationActivationReadinessService, OrganizationActivationReadinessService>();
            services.AddScoped<IOrganizationActivationReadinessRule, LegalProfileActivationRule>();
            services.AddScoped<IOrganizationActivationReadinessRule, OwnerActivationRule>();
            services.AddScoped<IOrganizationActivationReadinessRule, SubscriptionActivationRule>();
            services.AddScoped<IOrganizationActivationReadinessRule, OperationalSettingsActivationRule>();
            services.AddScoped<IOrganizationActivationReadinessRule, PrimaryBranchActivationRule>();

        services.AddMemoryCache();

services.Configure<OrganizationActivationReadinessCacheOptions>(options =>
    configuration
        .GetSection(OrganizationActivationReadinessCacheOptions.SectionName)
        .Bind(options));

services.AddSingleton<IOrganizationActivationReadinessReportCache,
    OrganizationActivationReadinessMemoryCache>();

services.AddScoped<IOrganizationActivationReadinessAuditSink,
    LoggerOrganizationActivationReadinessAuditSink>();

    services.AddScoped<
    IOrganizationActivationReadinessCacheInvalidator,
    OrganizationActivationReadinessCacheInvalidator>();

 services.AddScoped<IOrganizationClosureReadinessService, OrganizationClosureReadinessService>();
services.AddScoped<IOrganizationClosureReadinessSnapshotSource, OrganizationClosureReadinessSnapshotSource>();
services.AddScoped<IOrganizationClosureOrchestrator, OrganizationClosureOrchestrator>();
services.AddScoped<IOrganizationArchiveService, OrganizationArchiveService>();
services.AddScoped<IOrganizationAnonymizationService, OrganizationAnonymizationService>();
services.AddScoped<IOrganizationClosureAuditSink, OrganizationClosureAuditSink>();
services.AddScoped<IOrganizationClosureScheduler, OrganizationClosureScheduler>();
services.AddHostedService<OrganizationClosureWorker>();

        return services;
    }
}
