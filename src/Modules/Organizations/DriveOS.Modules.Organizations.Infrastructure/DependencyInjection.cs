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
    IBranchConfigurationOverrideRepository,
    BranchConfigurationOverrideRepository>();


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
        return services;
    }
}
