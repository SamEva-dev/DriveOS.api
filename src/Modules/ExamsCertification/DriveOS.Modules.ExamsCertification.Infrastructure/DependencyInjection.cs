using DriveOS.Modules.ExamsCertification.Application.Providers;
using DriveOS.Modules.ExamsCertification.Application.Analytics;
using DriveOS.Modules.ExamsCertification.Infrastructure.Providers;
using DriveOS.Modules.ExamsCertification.Infrastructure.Analytics;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Readiness;
using DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;
using DriveOS.Modules.ExamsCertification.Domain.Places;
using DriveOS.Modules.ExamsCertification.Domain.Places.Watch;
using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;
using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.File;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Submissions;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;
using DriveOS.Modules.ExamsCertification.Domain.Results;
using DriveOS.Modules.ExamsCertification.Domain.Results.Success;
using DriveOS.Modules.ExamsCertification.Domain.Results.Failure;
using DriveOS.Modules.ExamsCertification.Domain.Remediation;
using DriveOS.Modules.ExamsCertification.Domain.Certifications;
using DriveOS.Modules.ExamsCertification.Application.Providers.Connections;
using DriveOS.Modules.ExamsCertification.Application.Registrations.Submissions;
using DriveOS.Modules.ExamsCertification.Infrastructure.Persistence;
using DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;
using DriveOS.Modules.ExamsCertification.Infrastructure.Places.Watch;
using DriveOS.Modules.ExamsCertification.Application.Success;
using DriveOS.Modules.ExamsCertification.Infrastructure.Success;
using DriveOS.Modules.ExamsCertification.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.Modules.ExamsCertification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddExamsCertificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DriveOS")
            ?? throw new InvalidOperationException("The DriveOS database connection string is missing.");

        services.AddDbContext<ExamsCertificationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "exams_certification")));

        services.AddOptions<ExamPlaceWatcherOptions>()
            .Bind(configuration.GetSection(ExamPlaceWatcherOptions.SectionName))
            .Validate(x => x.PollSeconds is >= 1 and <= 3600, "PollSeconds must be between 1 and 3600.")
            .Validate(x => x.BatchSize is >= 1 and <= 500, "BatchSize must be between 1 and 500.")
            .Validate(x => x.ProcessingLeaseMinutes is >= 1 and <= 60, "ProcessingLeaseMinutes must be between 1 and 60.")
            .ValidateOnStart();

        services.AddOptions<ExamSuccessConsequencesOptions>()
            .Bind(configuration.GetSection(ExamSuccessConsequencesOptions.SectionName))
            .Validate(x => x.PollSeconds is >= 1 and <= 3600, "PollSeconds must be between 1 and 3600.")
            .Validate(x => x.BatchSize is >= 1 and <= 500, "BatchSize must be between 1 and 500.")
            .Validate(x => x.ProcessingLeaseMinutes is >= 1 and <= 60, "ProcessingLeaseMinutes must be between 1 and 60.")
            .Validate(x => x.DeferredRetryHours is >= 1 and <= 168, "DeferredRetryHours must be between 1 and 168.")
            .Validate(x => x.ExceptionRetryMinutes is >= 1 and <= 1440, "ExceptionRetryMinutes must be between 1 and 1440.")
            .Validate(x => x.MaxRetryMinutes is >= 1 and <= 1440, "MaxRetryMinutes must be between 1 and 1440.")
            .ValidateOnStart();

        services.AddOptions<ExamProviderExecutionOptions>()
            .Bind(configuration.GetSection(ExamProviderExecutionOptions.SectionName))
            .Validate(x => x.DefaultRequestsPerMinute is >= 1 and <= 600, "DefaultRequestsPerMinute must be between 1 and 600.")
            .Validate(x => x.MaxRequestsPerMinute is >= 1 and <= 5000, "MaxRequestsPerMinute must be between 1 and 5000.")
            .Validate(x => x.DefaultRequestsPerMinute <= x.MaxRequestsPerMinute, "DefaultRequestsPerMinute cannot exceed MaxRequestsPerMinute.")
            .Validate(x => x.CircuitFailureThreshold is >= 1 and <= 100, "CircuitFailureThreshold must be between 1 and 100.")
            .Validate(x => x.CircuitOpenMinutes is >= 1 and <= 1440, "CircuitOpenMinutes must be between 1 and 1440.")
            .ValidateOnStart();

        services.AddOptions<ExamAnalyticsOptions>()
            .Bind(configuration.GetSection(ExamAnalyticsOptions.SectionName))
            .Validate(x => x.SmallSampleThreshold is >= 1 and <= 1000, "SmallSampleThreshold must be between 1 and 1000.")
            .Validate(x => x.DefaultPeriodMonths is >= 1 and <= 120, "DefaultPeriodMonths must be between 1 and 120.")
            .Validate(x => x.PassRateDropAlertPoints is >= 0 and <= 100, "PassRateDropAlertPoints must be between 0 and 100.")
            .Validate(x => x.ContextualUnderperformancePoints is >= 0 and <= 100, "ContextualUnderperformancePoints must be between 0 and 100.")
            .Validate(x => x.RecurrentFailureReasonPercent is >= 0 and <= 100, "RecurrentFailureReasonPercent must be between 0 and 100.")
            .ValidateOnStart();

        services.AddScoped<IExamsCertificationUnitOfWork>(sp =>
            sp.GetRequiredService<ExamsCertificationDbContext>());
        services.AddScoped<IExamReadinessDecisionRepository, ExamReadinessDecisionRepository>();
        services.AddScoped<IExamReadinessOpinionRepository, ExamReadinessOpinionRepository>();
        services.AddScoped<IExamCenterRepository, ExamCenterRepository>();
        services.AddScoped<IExamPlaceRepository, ExamPlaceRepository>();
        services.AddScoped<IExamPlaceWatchRepository, ExamPlaceWatchRepository>();
        services.AddScoped<IExamProviderConnectionRepository, ExamProviderConnectionRepository>();
        services.AddScoped<IExamRegistrationRepository, ExamRegistrationRepository>();
        services.AddScoped<IExamRegistrationFileRepository, ExamRegistrationFileRepository>();
        services.AddScoped<IExamRegistrationSubmissionRepository, ExamRegistrationSubmissionRepository>();
        services.AddScoped<IExamConvocationRepository, ExamConvocationRepository>();
        services.AddScoped<IExamOperationalPlanRepository, ExamOperationalPlanRepository>();
        services.AddScoped<IExamResourceAssignmentRepository, ExamResourceAssignmentRepository>();
        services.AddScoped<IExamPreparationRepository, ExamPreparationRepository>();
        services.AddScoped<IExamAttemptRepository, ExamAttemptRepository>();
        services.AddScoped<IExamResultRepository, ExamResultRepository>();
        services.AddScoped<IExamSuccessProcessRepository, ExamSuccessProcessRepository>();
        services.AddScoped<IExamFailureAnalysisRepository, ExamFailureAnalysisRepository>();
        services.AddScoped<IExamRemediationRequestRepository, ExamRemediationRequestRepository>();
        services.AddScoped<IExamAttestationRepository, ExamAttestationRepository>();
        services.AddScoped<IExamAnalyticsReadService, ExamAnalyticsReadService>();
        services.AddScoped<IExamSuccessConsequenceStore, ExamSuccessConsequenceStore>();
        services.AddHostedService<ExamSuccessConsequenceWorker>();
        services.AddScoped<IExamProviderErrorMapper, DefaultExamProviderErrorMapper>();
        services.AddScoped<IExamProviderConnectionTester, ExamProviderConnectionTester>();
        services.AddScoped<IExamProviderExecutionGuard, ExamProviderExecutionGuard>();
        services.AddSingleton<IExamPlaceProvider, ManualExamPlaceProvider>();
        services.AddSingleton<IExamRegistrationSubmissionProvider, ManualExamRegistrationSubmissionProvider>();
        // Integration slots are intentionally disabled until an authorized endpoint/specification and credentials are configured.
        // No private/reverse-engineered RdvPermis endpoint is embedded in DriveOS.
        services.AddSingleton<IExamPlaceProvider>(_ => new ExternalExamProviderPlaceholder(
            "rdvpermis", "FR", ExamPlaceProviderKind.OfficialApi,
            ExamPlaceProviderCapability.ReadAvailablePlaces | ExamPlaceProviderCapability.ReadAssignedPlaces |
            ExamPlaceProviderCapability.WatchAvailability | ExamPlaceProviderCapability.ReservePlace |
            ExamPlaceProviderCapability.ReleasePlace | ExamPlaceProviderCapability.SubmitRegistration |
            ExamPlaceProviderCapability.ReadRegistrationStatus | ExamPlaceProviderCapability.ReadResults));
        services.AddSingleton<IExamPlaceProvider>(_ => new ExternalExamProviderPlaceholder(
            "authorized-partner", "*", ExamPlaceProviderKind.AuthorizedPartnerApi,
            ExamPlaceProviderCapability.ReadAvailablePlaces | ExamPlaceProviderCapability.ReadAssignedPlaces |
            ExamPlaceProviderCapability.WatchAvailability));
        services.AddSingleton<IExamPlaceProvider>(_ => new ExternalExamProviderPlaceholder(
            "local-agent", "*", ExamPlaceProviderKind.BrowserAgent,
            ExamPlaceProviderCapability.ReadAvailablePlaces | ExamPlaceProviderCapability.ReadAssignedPlaces |
            ExamPlaceProviderCapability.WatchAvailability));
        services.AddSingleton<IExamRegistrationSubmissionProvider>(_ => new ExternalExamProviderPlaceholder(
            "rdvpermis", "FR", ExamPlaceProviderKind.OfficialApi,
            ExamPlaceProviderCapability.SubmitRegistration | ExamPlaceProviderCapability.ReadRegistrationStatus));
        services.AddSingleton<IExamRegistrationSubmissionProvider>(_ => new ExternalExamProviderPlaceholder(
            "authorized-partner", "*", ExamPlaceProviderKind.AuthorizedPartnerApi,
            ExamPlaceProviderCapability.SubmitRegistration | ExamPlaceProviderCapability.ReadRegistrationStatus));
        services.AddSingleton<IExamRegistrationSubmissionProvider>(_ => new ExternalExamProviderPlaceholder(
            "local-agent", "*", ExamPlaceProviderKind.BrowserAgent,
            ExamPlaceProviderCapability.SubmitRegistration | ExamPlaceProviderCapability.ReadRegistrationStatus));
        services.AddSingleton<IExamPlaceProviderResolver, ExamPlaceProviderResolver>();
        services.AddSingleton<IExamRegistrationSubmissionProviderResolver, ExamRegistrationSubmissionProviderResolver>();
        services.AddHostedService<ExamPlaceWatchWorker>();
        return services;
    }
}
