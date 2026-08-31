using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Applications;
using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Proposals;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.CommercialOffers;

public sealed class CreateProfessionalCommercialOfferCommandHandler(
    IProfessionalCommercialOfferRepository offers,
    IProfessionalApplicationRepository applications,
    IProfessionalProposalRepository proposals,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<CreateProfessionalCommercialOfferCommand,ProfessionalCommercialOfferId>
{
    public async Task<Result<ProfessionalCommercialOfferId>> Handle(CreateProfessionalCommercialOfferCommand c,CancellationToken ct)
    {
        if(c.ApplicationId is ProfessionalApplicationId appId)
        {
            if(await offers.ActiveOfferExistsForApplicationAsync(appId,ct))
                return Result.Failure<ProfessionalCommercialOfferId>(Error.Conflict("ProfessionalMarketplace.CommercialOffers.ActiveOfferExists","errors.professionalMarketplace.commercialOffers.activeOfferExists"));
            var application=await applications.GetAsync(appId,false,ct);
            if(application is null||application.Status!=ProfessionalApplicationStatus.Accepted||application.OrganizationId!=c.OrganizationId||application.ProfessionalProfileId!=c.ProfessionalProfileId)
                return Result.Failure<ProfessionalCommercialOfferId>(ProfessionalCommercialOfferErrors.InvalidSource);
        }
        else if(c.ProposalId is ProfessionalProposalId proposalId)
        {
            if(await offers.ActiveOfferExistsForProposalAsync(proposalId,ct))
                return Result.Failure<ProfessionalCommercialOfferId>(Error.Conflict("ProfessionalMarketplace.CommercialOffers.ActiveOfferExists","errors.professionalMarketplace.commercialOffers.activeOfferExists"));
            var proposal=await proposals.GetAsync(proposalId,false,ct);
            if(proposal is null||proposal.Status!=ProfessionalProposalStatus.Accepted||proposal.OrganizationId!=c.OrganizationId||proposal.ProfessionalProfileId!=c.ProfessionalProfileId)
                return Result.Failure<ProfessionalCommercialOfferId>(ProfessionalCommercialOfferErrors.InvalidSource);
        }

        var created=ProfessionalCommercialOffer.Create(c.Id,c.OrganizationId,c.ProfessionalProfileId,c.ApplicationId,c.ProposalId,c.OpportunityId,c.Terms,clock.UtcNow,c.ActorUserId);
        if(created.IsFailure)return Result.Failure<ProfessionalCommercialOfferId>(created.Error);
        offers.Add(created.Value);await uow.CommitAsync(ct);return Result.Success(created.Value.Id);
    }
}

public abstract class CommercialOfferMutation
{
    protected static async Task<Result> ForOrganization(ProfessionalCommercialOfferId id,OrganizationId org,Func<ProfessionalCommercialOffer,Result> mutate,IProfessionalCommercialOfferRepository repo,IProfessionalMarketplaceUnitOfWork uow,CancellationToken ct)
    {
        var x=await repo.GetAsync(id,true,ct);if(x is null||x.OrganizationId!=org)return Result.Failure(ProfessionalCommercialOfferErrors.NotFound);
        var r=mutate(x);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
    protected static async Task<Result> ForProfessional(ProfessionalCommercialOfferId id,ProfessionalProfileId profile,Func<ProfessionalCommercialOffer,Result> mutate,IProfessionalCommercialOfferRepository repo,IProfessionalMarketplaceUnitOfWork uow,CancellationToken ct)
    {
        var x=await repo.GetAsync(id,true,ct);if(x is null||x.ProfessionalProfileId!=profile)return Result.Failure(ProfessionalCommercialOfferErrors.NotFound);
        var r=mutate(x);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}
public sealed class ReviseProfessionalCommercialOfferCommandHandler(IProfessionalCommercialOfferRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):CommercialOfferMutation,ICommandHandler<ReviseProfessionalCommercialOfferCommand>
{public Task<Result> Handle(ReviseProfessionalCommercialOfferCommand c,CancellationToken ct)=>ForOrganization(c.Id,c.OrganizationId,x=>x.Revise(c.Terms,clock.UtcNow,c.ActorUserId),repo,uow,ct);}
public sealed class SendProfessionalCommercialOfferCommandHandler(IProfessionalCommercialOfferRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):CommercialOfferMutation,ICommandHandler<SendProfessionalCommercialOfferCommand>
{public Task<Result> Handle(SendProfessionalCommercialOfferCommand c,CancellationToken ct)=>ForOrganization(c.Id,c.OrganizationId,x=>x.Send(clock.UtcNow,c.ActorUserId),repo,uow,ct);}
public sealed class AcceptCommercialOfferByOrganizationCommandHandler(IProfessionalCommercialOfferRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):CommercialOfferMutation,ICommandHandler<AcceptCommercialOfferByOrganizationCommand>
{public Task<Result> Handle(AcceptCommercialOfferByOrganizationCommand c,CancellationToken ct)=>ForOrganization(c.Id,c.OrganizationId,x=>x.AcceptByOrganization(clock.UtcNow,c.ActorUserId),repo,uow,ct);}
public sealed class AcceptCommercialOfferByProfessionalCommandHandler(IProfessionalCommercialOfferRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):CommercialOfferMutation,ICommandHandler<AcceptCommercialOfferByProfessionalCommand>
{public Task<Result> Handle(AcceptCommercialOfferByProfessionalCommand c,CancellationToken ct)=>ForProfessional(c.Id,c.ProfileId,x=>x.AcceptByProfessional(clock.UtcNow,c.ActorUserId),repo,uow,ct);}
public sealed class FinalizeProfessionalCommercialOfferCommandHandler(IProfessionalCommercialOfferRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):CommercialOfferMutation,ICommandHandler<FinalizeProfessionalCommercialOfferCommand>
{public Task<Result> Handle(FinalizeProfessionalCommercialOfferCommand c,CancellationToken ct)=>ForOrganization(c.Id,c.OrganizationId,x=>x.FinalizeOffer(clock.UtcNow,c.ActorUserId),repo,uow,ct);}
public sealed class CancelProfessionalCommercialOfferCommandHandler(IProfessionalCommercialOfferRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):CommercialOfferMutation,ICommandHandler<CancelProfessionalCommercialOfferCommand>
{public Task<Result> Handle(CancelProfessionalCommercialOfferCommand c,CancellationToken ct)=>ForOrganization(c.Id,c.OrganizationId,x=>x.Cancel(c.Reason,clock.UtcNow,c.ActorUserId),repo,uow,ct);}

public sealed class ListProfessionalCommercialOffersQueryHandler(
    IProfessionalCommercialOfferRepository repo)
    :IQueryHandler<ListProfessionalCommercialOffersQuery,IReadOnlyList<ProfessionalCommercialOfferResponse>>
{
    public async Task<Result<IReadOnlyList<ProfessionalCommercialOfferResponse>>> Handle(
        ListProfessionalCommercialOffersQuery q,
        CancellationToken ct)
    {
        var rows=await repo.ListAsync(q.OrganizationId,q.ProfessionalProfileId,q.ApplicationId,q.ProposalId,q.OpportunityId,ct);
        IReadOnlyList<ProfessionalCommercialOfferResponse> response=rows.Select(x=>new ProfessionalCommercialOfferResponse(
            x.Id.Value,
            x.OrganizationId.Value,
            x.ProfessionalProfileId.Value,
            x.ApplicationId?.Value,
            x.ProposalId?.Value,
            x.OpportunityId?.Value,
            x.Terms,
            x.Revision,
            x.Status.ToString(),
            x.SentAtUtc,
            x.OrganizationAcceptedAtUtc,
            x.ProfessionalAcceptedAtUtc,
            x.FinalizedAtUtc,
            x.OrganizationAcceptedByUserId?.Value,
            x.ProfessionalAcceptedByUserId?.Value,
            x.CancellationReason,
            x.CreatedAtUtc,
            x.RevisionHistory.OrderByDescending(r=>r.Revision).Select(r=>new ProfessionalCommercialOfferRevisionResponse(
                r.Revision,r.Terms,r.ChangedAtUtc,r.ChangedByUserId.Value)).ToArray())).ToArray();
        return Result.Success(response);
    }
}
