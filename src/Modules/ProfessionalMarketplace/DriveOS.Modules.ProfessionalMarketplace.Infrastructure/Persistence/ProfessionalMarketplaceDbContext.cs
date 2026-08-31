using DriveOS.Modules.ProfessionalMarketplace.Domain.Invitations;
using DriveOS.Modules.ProfessionalMarketplace.Domain.StudentAssignments;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence;

public sealed class ProfessionalMarketplaceDbContext(DbContextOptions<ProfessionalMarketplaceDbContext> options)
    : DbContext(options), IProfessionalMarketplaceUnitOfWork
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<UserId>().HaveConversion<UserIdConverter>();
    }

    public DbSet<ProfessionalProfile> ProfessionalProfiles => Set<ProfessionalProfile>();
    public DbSet<ProfessionalDocument> ProfessionalDocuments => Set<ProfessionalDocument>();
    public DbSet<ProfessionalCredential> ProfessionalCredentials => Set<ProfessionalCredential>();
    public DbSet<ProfessionalComplianceCriticalityPolicy> ProfessionalComplianceCriticalityPolicies => Set<ProfessionalComplianceCriticalityPolicy>();
    public DbSet<ProfessionalComplianceWaiver> ProfessionalComplianceWaivers => Set<ProfessionalComplianceWaiver>();
    public DbSet<ProfessionalComplianceRequirement> ProfessionalComplianceRequirements => Set<ProfessionalComplianceRequirement>();
    public DbSet<ProfessionalOpportunity> ProfessionalOpportunities => Set<ProfessionalOpportunity>();
    public DbSet<ProfessionalApplication> ProfessionalApplications => Set<ProfessionalApplication>();
    public DbSet<ProfessionalProposal> ProfessionalProposals => Set<ProfessionalProposal>();
    public DbSet<ProfessionalCommercialOffer> ProfessionalCommercialOffers => Set<ProfessionalCommercialOffer>();
    public DbSet<ProfessionalEngagement> ProfessionalEngagements => Set<ProfessionalEngagement>();
    public DbSet<ProfessionalReview> ProfessionalReviews => Set<ProfessionalReview>();
    public DbSet<ProfessionalReviewReport> ProfessionalReviewReports => Set<ProfessionalReviewReport>();
    public DbSet<ProfessionalMission> ProfessionalMissions => Set<ProfessionalMission>();
    public DbSet<FreelanceInvitation> FreelanceInvitations => Set<FreelanceInvitation>();
    public DbSet<ProfessionalStudentAssignment> ProfessionalStudentAssignments => Set<ProfessionalStudentAssignment>();
    public DbSet<ExternalAccessGrant> ExternalAccessGrants => Set<ExternalAccessGrant>();
    public DbSet<ServiceEntry> ServiceEntries => Set<ServiceEntry>();
    public DbSet<ServiceDispute> ServiceDisputes => Set<ServiceDispute>();
    public DbSet<ServiceStatement> ServiceStatements => Set<ServiceStatement>();
    public DbSet<ProfessionalInvoice> ProfessionalInvoices => Set<ProfessionalInvoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(ProfessionalMarketplaceSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProfessionalMarketplaceDbContext).Assembly);
        ApplyUserIdConversions(modelBuilder);
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(cancellationToken);

    private static void ApplyUserIdConversions(ModelBuilder modelBuilder)
    {
        var required=new ValueConverter<UserId,Guid>(x=>x.Value,x=>new UserId(x));
        var optional=new ValueConverter<UserId?,Guid?>(x=>x.HasValue?x.Value.Value:null,x=>x.HasValue?new UserId(x.Value):null);
        foreach(var property in modelBuilder.Model.GetEntityTypes().SelectMany(x=>x.GetProperties()))
        {
            if(property.GetValueConverter() is not null)continue;
            if(property.ClrType==typeof(UserId))property.SetValueConverter(required);
            else if(property.ClrType==typeof(UserId?))property.SetValueConverter(optional);
        }
    }

    private sealed class UserIdConverter():ValueConverter<UserId,Guid>(x=>x.Value,x=>new UserId(x));
}
