using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Applications;
using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Proposals;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Results;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Messaging;

public sealed class EnsureMarketplaceConversationCommandHandler(
    IProfessionalOpportunityRepository opportunities,
    IProfessionalApplicationRepository applications,
    IProfessionalProposalRepository proposals,
    IProfessionalCommercialOfferRepository offers,
    IProfessionalEngagementRepository engagements,
    IProfessionalProfileRepository profiles,
    IMarketplaceCommunicationGateway communication) : ICommandHandler<EnsureMarketplaceConversationCommand,Guid>
{
    public async Task<Result<Guid>> Handle(EnsureMarketplaceConversationCommand c,CancellationToken ct)
    {
        (ProfessionalProfileId profileId,string relatedType,Guid relatedId)? context=await Resolve(c,ct);
        if(context is null)return Result.Failure<Guid>(ProfessionalProfileErrors.NotFound);
        ProfessionalProfile? profile=await profiles.GetByIdAsync(context.Value.profileId,ct);
        if(profile is null||profile.UserId.HasValue && profile.UserId.Value.IsEmpty)
            return Result.Failure<Guid>(ProfessionalProfileErrors.NotFound);

        var profiluserId = profile.UserId.HasValue ? profile.UserId.Value : default;
        Guid id=await communication.EnsureConversationAsync(c.OrganizationId,context.Value.relatedType,context.Value.relatedId,profiluserId,c.ActorUserId,ct);
        return Result.Success(id);
    }

    private async Task<(ProfessionalProfileId profileId,string relatedType,Guid relatedId)?> Resolve(EnsureMarketplaceConversationCommand c,CancellationToken ct)
    {
        switch(c.ContextType)
        {
            case MarketplaceConversationContextType.Opportunity:
            {
                if(c.ProfessionalProfileId is not ProfessionalProfileId profileId)return null;
                ProfessionalOpportunity? x=await opportunities.GetAsync(new(c.ContextId),false,ct);
                if(x is null||x.OrganizationId!=c.OrganizationId||x.Status!=ProfessionalOpportunityStatus.Published)return null;
                return (profileId,"PROFESSIONAL_OPPORTUNITY",x.Id.Value);
            }
            case MarketplaceConversationContextType.Application:
            {
                ProfessionalApplication? x=await applications.GetAsync(new(c.ContextId),false,ct);
                return x is not null&&x.OrganizationId==c.OrganizationId?(x.ProfessionalProfileId,"PROFESSIONAL_APPLICATION",x.Id.Value):null;
            }
            case MarketplaceConversationContextType.Proposal:
            {
                ProfessionalProposal? x=await proposals.GetAsync(new(c.ContextId),false,ct);
                return x is not null&&x.OrganizationId==c.OrganizationId?(x.ProfessionalProfileId,"PROFESSIONAL_PROPOSAL",x.Id.Value):null;
            }
            case MarketplaceConversationContextType.CommercialOffer:
            {
                ProfessionalCommercialOffer? x=await offers.GetAsync(new(c.ContextId),false,ct);
                return x is not null&&x.OrganizationId==c.OrganizationId?(x.ProfessionalProfileId,"PROFESSIONAL_COMMERCIAL_OFFER",x.Id.Value):null;
            }
            case MarketplaceConversationContextType.Engagement:
            {
                ProfessionalEngagement? x=await engagements.GetAsync(new(c.ContextId),false,ct);
                return x is not null&&x.OrganizationId==c.OrganizationId?(x.ProfessionalProfileId,"PROFESSIONAL_ENGAGEMENT",x.Id.Value):null;
            }
            default:return null;
        }
    }
}
