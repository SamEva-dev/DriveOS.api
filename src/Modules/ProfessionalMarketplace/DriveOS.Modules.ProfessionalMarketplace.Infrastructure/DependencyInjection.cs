using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Applications;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Proposals;
using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Reviews;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceStatements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Invoices;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Disputes;
using DriveOS.Modules.ProfessionalMarketplace.Domain.StudentAssignments;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Invitations;
using DriveOS.Modules.ProfessionalMarketplace.Application.Search;
using DriveOS.Modules.ProfessionalMarketplace.Application.Matching;
using DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Application.Dashboard;
using DriveOS.Modules.ProfessionalMarketplace.Application.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Outbox;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProfessionalMarketplaceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string cs = configuration.GetConnectionString("DriveOS")
            ?? throw new InvalidOperationException("The DriveOS database connection string is missing.");

        services.AddScoped<MarketplaceOutboxInterceptor>();
        services.AddDbContext<ProfessionalMarketplaceDbContext>((provider, options) =>
        {
            options.UseNpgsql(cs, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", ProfessionalMarketplaceSchema.Name));
            options.AddInterceptors(provider.GetRequiredService<MarketplaceOutboxInterceptor>());
        });
        services.AddScoped<IProfessionalMarketplaceUnitOfWork>(sp => sp.GetRequiredService<ProfessionalMarketplaceDbContext>());
        services.AddScoped<IProfessionalProfileRepository, ProfessionalProfileRepository>();
        services.AddScoped<IProfessionalDocumentRepository, ProfessionalDocumentRepository>();
        services.AddScoped<IProfessionalCredentialRepository, ProfessionalCredentialRepository>();
        services.AddScoped<IProfessionalCompliancePolicyRepository, ProfessionalCompliancePolicyRepository>();
        services.AddScoped<IProfessionalComplianceWaiverRepository, ProfessionalComplianceWaiverRepository>();
        services.AddScoped<IProfessionalComplianceRequirementRepository, ProfessionalComplianceRequirementRepository>();
        services.AddScoped<IProfessionalSearchReadService, ProfessionalSearchReadService>();
        services.AddScoped<IProfessionalMatchingReadService, ProfessionalMatchingReadService>();
        services.AddScoped<IProfessionalEngagementOperationalReadService, ProfessionalEngagementOperationalReadService>();
        services.AddScoped<IProfessionalOpportunityRepository, ProfessionalOpportunityRepository>();
        services.AddScoped<IProfessionalApplicationRepository, ProfessionalApplicationRepository>();
        services.AddScoped<IProfessionalProposalRepository, ProfessionalProposalRepository>();
        services.AddScoped<IProfessionalCommercialOfferRepository, ProfessionalCommercialOfferRepository>();
        services.AddScoped<IProfessionalEngagementRepository, ProfessionalEngagementRepository>();
        services.AddScoped<IProfessionalMissionRepository, ProfessionalMissionRepository>();
        services.AddScoped<IExternalAccessGrantRepository, ExternalAccessGrantRepository>();
        services.AddScoped<IServiceEntryRepository, ServiceEntryRepository>();
        services.AddScoped<IServiceDisputeRepository, ServiceDisputeRepository>();
        services.AddScoped<IServiceStatementRepository, ServiceStatementRepository>();
        services.AddScoped<IProfessionalInvoiceRepository, ProfessionalInvoiceRepository>();
        services.AddScoped<IProfessionalStudentAssignmentRepository, ProfessionalStudentAssignmentRepository>();
        services.AddScoped<IFreelanceInvitationRepository, FreelanceInvitationRepository>();
        services.AddScoped<IProfessionalReviewRepository, ProfessionalReviewRepository>();
        services.AddScoped<IProfessionalReviewReportRepository, ProfessionalReviewReportRepository>();
        services.AddScoped<IMarketplaceDashboardReadService, MarketplaceDashboardReadService>();
        services.AddScoped<IProfessionalComplianceExpirationAutomation, ProfessionalComplianceExpirationAutomation>();
        return services;
    }
}
