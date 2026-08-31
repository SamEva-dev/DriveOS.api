using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Proposals;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Results;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Proposals;

public sealed class CreateProfessionalProposalCommandHandler(
    IProfessionalProposalRepository proposals,
    IProfessionalProfileRepository profiles,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<CreateProfessionalProposalCommand,ProfessionalProposalId>
{
    public async Task<Result<ProfessionalProposalId>> Handle(CreateProfessionalProposalCommand c,CancellationToken ct)
    {
        if(await proposals.OpenProposalExistsAsync(c.OrganizationId,c.ProfessionalProfileId,c.OpportunityId,ct))
            return Result.Failure<ProfessionalProposalId>(ProfessionalProposalErrors.DuplicateOpenProposal);
        var profile=await profiles.GetByIdAsync(c.ProfessionalProfileId,ct);
        if(profile is null)return Result.Failure<ProfessionalProposalId>(ProfessionalProfileErrors.NotFound);
        var created=ProfessionalProposal.Create(
            c.Id,c.OrganizationId,c.BranchId,profile,c.OpportunityId,c.Subject,c.Message,c.StartsOn,c.EndsOn,
            c.TeachingCategoryCodes,c.EngagementType,c.VehicleProvisionMode,c.ProposedRate,c.Currency,c.RateUnit,
            c.Negotiable,c.ExpiresAtUtc,clock.UtcNow,c.ActorUserId);
        if(created.IsFailure)return Result.Failure<ProfessionalProposalId>(created.Error);
        proposals.Add(created.Value);await uow.CommitAsync(ct);return Result.Success(created.Value.Id);
    }
}

public abstract class ProfessionalProposalMutationHandler
{
    protected static async Task<Result> ForProfile(ProfessionalProposalId id,ProfessionalProfileId profileId,Func<ProfessionalProposal,Result> mutate,IProfessionalProposalRepository repo,IProfessionalMarketplaceUnitOfWork uow,CancellationToken ct)
    {
        var x=await repo.GetAsync(id,true,ct);
        if(x is null||x.ProfessionalProfileId!=profileId)return Result.Failure(ProfessionalProposalErrors.NotFound);
        var r=mutate(x);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
    protected static async Task<Result> ForOrganization(ProfessionalProposalId id,OrganizationId organizationId,Func<ProfessionalProposal,Result> mutate,IProfessionalProposalRepository repo,IProfessionalMarketplaceUnitOfWork uow,CancellationToken ct)
    {
        var x=await repo.GetAsync(id,true,ct);
        if(x is null||x.OrganizationId!=organizationId)return Result.Failure(ProfessionalProposalErrors.NotFound);
        var r=mutate(x);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}
public sealed class AcceptProfessionalProposalCommandHandler(IProfessionalProposalRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ProfessionalProposalMutationHandler,ICommandHandler<AcceptProfessionalProposalCommand>
{public Task<Result> Handle(AcceptProfessionalProposalCommand c,CancellationToken ct)=>ForProfile(c.Id,c.ProfileId,x=>x.Accept(clock.UtcNow,c.ActorUserId),repo,uow,ct);}
public sealed class RejectProfessionalProposalCommandHandler(IProfessionalProposalRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ProfessionalProposalMutationHandler,ICommandHandler<RejectProfessionalProposalCommand>
{public Task<Result> Handle(RejectProfessionalProposalCommand c,CancellationToken ct)=>ForProfile(c.Id,c.ProfileId,x=>x.Reject(c.Reason,clock.UtcNow,c.ActorUserId),repo,uow,ct);}
public sealed class CounterProfessionalProposalCommandHandler(IProfessionalProposalRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ProfessionalProposalMutationHandler,ICommandHandler<CounterProfessionalProposalCommand>
{public Task<Result> Handle(CounterProfessionalProposalCommand c,CancellationToken ct)=>ForProfile(c.Id,c.ProfileId,x=>x.Counter(c.ProposedRate,c.Currency,c.RateUnit,c.Negotiable,c.Message,clock.UtcNow,c.ActorUserId),repo,uow,ct);}
public sealed class WithdrawProfessionalProposalCommandHandler(IProfessionalProposalRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ProfessionalProposalMutationHandler,ICommandHandler<WithdrawProfessionalProposalCommand>
{public Task<Result> Handle(WithdrawProfessionalProposalCommand c,CancellationToken ct)=>ForOrganization(c.Id,c.OrganizationId,x=>x.Withdraw(c.Reason,clock.UtcNow,c.ActorUserId),repo,uow,ct);}


public sealed class ListProfessionalProposalsQueryHandler(IProfessionalProposalRepository repo):IQueryHandler<ListProfessionalProposalsQuery,IReadOnlyList<ProfessionalProposalResponse>>
{
    public async Task<Result<IReadOnlyList<ProfessionalProposalResponse>>> Handle(ListProfessionalProposalsQuery q,CancellationToken ct)
    {
        var items=await repo.ListAsync(q.OrganizationId,q.ProfessionalProfileId,q.OpportunityId,ct);
        IReadOnlyList<ProfessionalProposalResponse> result=items.Select(x=>new ProfessionalProposalResponse(
            x.Id.Value,x.OrganizationId.Value,x.BranchId?.Value,x.ProfessionalProfileId.Value,x.OpportunityId?.Value,x.Subject,x.Message,x.StartsOn,x.EndsOn,x.TeachingCategoryCodes,x.EngagementType,x.VehicleProvisionMode,x.ProposedRate,x.Currency,x.RateUnit,x.Negotiable,x.ExpiresAtUtc,x.Status.ToString(),x.Revision,x.DecisionReason,x.SentAtUtc,x.RespondedAtUtc,
            x.RevisionHistory.OrderByDescending(r=>r.Revision).Select(r=>new ProfessionalProposalRevisionResponse(r.Revision,r.Subject,r.Message,r.StartsOn,r.EndsOn,r.TeachingCategoryCodes,r.EngagementType,r.VehicleProvisionMode,r.ProposedRate,r.Currency,r.RateUnit,r.Negotiable,r.ChangedAtUtc,r.ChangedByUserId.Value)).ToArray())).ToArray();
        return Result.Success(result);
    }
}
