using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Applications;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Applications;

public sealed class SubmitProfessionalApplicationCommandHandler(
    IProfessionalApplicationRepository applications,
    IProfessionalOpportunityRepository opportunities,
    IProfessionalProfileRepository profiles,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<SubmitProfessionalApplicationCommand,ProfessionalApplicationId>
{
    public async Task<Result<ProfessionalApplicationId>> Handle(SubmitProfessionalApplicationCommand c,CancellationToken ct)
    {
        if(await applications.ExistsAsync(c.OpportunityId,c.ProfessionalProfileId,ct))
            return Result.Failure<ProfessionalApplicationId>(ProfessionalApplicationErrors.DuplicateApplication);
        var opportunity=await opportunities.GetAsync(c.OpportunityId,false,ct);
        if(opportunity is null)return Result.Failure<ProfessionalApplicationId>(ProfessionalOpportunityErrors.NotFound);
        var profile=await profiles.GetByIdAsync(c.ProfessionalProfileId,ct);
        if(profile is null)return Result.Failure<ProfessionalApplicationId>(ProfessionalProfileErrors.NotFound);

        var created=ProfessionalApplication.Create(c.Id,opportunity,profile,c.Message,c.ProposedRate,c.Currency,c.RateUnit,c.Negotiable,c.AvailableFrom,c.AvailableUntil,clock.UtcNow,c.ActorUserId);
        if(created.IsFailure)return Result.Failure<ProfessionalApplicationId>(created.Error);
        applications.Add(created.Value);await uow.CommitAsync(ct);return Result.Success(created.Value.Id);
    }
}

public abstract class ProfessionalApplicationMutationHandler
{
    protected static async Task<Result> ForOrganization(ProfessionalApplicationId id,OrganizationId organizationId,Func<ProfessionalApplication,Result> mutate,IProfessionalApplicationRepository repo,IProfessionalMarketplaceUnitOfWork uow,CancellationToken ct)
    {
        var x=await repo.GetAsync(id,true,ct);
        if(x is null||x.OrganizationId!=organizationId)return Result.Failure(ProfessionalApplicationErrors.NotFound);
        var r=mutate(x);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}
public sealed class ReviewProfessionalApplicationCommandHandler(IProfessionalApplicationRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ProfessionalApplicationMutationHandler,ICommandHandler<ReviewProfessionalApplicationCommand>
{public Task<Result> Handle(ReviewProfessionalApplicationCommand c,CancellationToken ct)=>ForOrganization(c.Id,c.OrganizationId,x=>x.StartReview(clock.UtcNow,c.ActorUserId),repo,uow,ct);}
public sealed class ShortlistProfessionalApplicationCommandHandler(IProfessionalApplicationRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ProfessionalApplicationMutationHandler,ICommandHandler<ShortlistProfessionalApplicationCommand>
{public Task<Result> Handle(ShortlistProfessionalApplicationCommand c,CancellationToken ct)=>ForOrganization(c.Id,c.OrganizationId,x=>x.Shortlist(clock.UtcNow,c.ActorUserId),repo,uow,ct);}
public sealed class AcceptProfessionalApplicationCommandHandler(IProfessionalApplicationRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ProfessionalApplicationMutationHandler,ICommandHandler<AcceptProfessionalApplicationCommand>
{public Task<Result> Handle(AcceptProfessionalApplicationCommand c,CancellationToken ct)=>ForOrganization(c.Id,c.OrganizationId,x=>x.Accept(clock.UtcNow,c.ActorUserId),repo,uow,ct);}
public sealed class RejectProfessionalApplicationCommandHandler(IProfessionalApplicationRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ProfessionalApplicationMutationHandler,ICommandHandler<RejectProfessionalApplicationCommand>
{public Task<Result> Handle(RejectProfessionalApplicationCommand c,CancellationToken ct)=>ForOrganization(c.Id,c.OrganizationId,x=>x.Reject(c.Reason,clock.UtcNow,c.ActorUserId),repo,uow,ct);}
public sealed class WithdrawProfessionalApplicationCommandHandler(IProfessionalApplicationRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<WithdrawProfessionalApplicationCommand>
{
    public async Task<Result> Handle(WithdrawProfessionalApplicationCommand c,CancellationToken ct)
    {
        var x=await repo.GetAsync(c.Id,true,ct);if(x is null||x.ProfessionalProfileId!=c.ProfileId)return Result.Failure(ProfessionalApplicationErrors.NotFound);
        var r=x.Withdraw(c.Reason,clock.UtcNow,c.ActorUserId);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}


public sealed class ListProfessionalApplicationsQueryHandler(
    IProfessionalApplicationRepository applications,
    IProfessionalOpportunityRepository opportunities,
    IProfessionalProfileRepository profiles):IQueryHandler<ListProfessionalApplicationsQuery,IReadOnlyList<ProfessionalApplicationResponse>>
{
    public async Task<Result<IReadOnlyList<ProfessionalApplicationResponse>>> Handle(ListProfessionalApplicationsQuery q,CancellationToken ct)
    {
        var opportunity=await opportunities.GetAsync(q.OpportunityId,false,ct);
        if(opportunity is null||opportunity.OrganizationId!=q.OrganizationId)
            return Result.Failure<IReadOnlyList<ProfessionalApplicationResponse>>(ProfessionalOpportunityErrors.NotFound);

        var items=await applications.ListByOpportunityAsync(q.OpportunityId,ct);
        var result=new List<ProfessionalApplicationResponse>(items.Count);
        foreach(var item in items.OrderByDescending(x=>x.SubmittedAtUtc))
        {
            var profile=await profiles.GetByIdAsync(item.ProfessionalProfileId,ct);
            if(profile is null) continue;
            result.Add(new ProfessionalApplicationResponse(
                item.Id.Value,item.OpportunityId.Value,item.ProfessionalProfileId.Value,item.Status.ToString(),item.Message,
                item.ProposedRate,item.Currency,item.RateUnit?.ToString(),item.Negotiable,item.AvailableFrom,item.AvailableUntil,
                item.DecisionReason,item.SubmittedAtUtc,item.DecidedAtUtc,
                profile.TradeName??profile.LegalName??profile.Headline??"Professional",profile.Headline,profile.ExperienceYears,
                profile.ComplianceStatus.ToString(),profile.TeachingCategoryCodes,profile.Languages,profile.PrimaryServiceArea));
        }
        return Result.Success<IReadOnlyList<ProfessionalApplicationResponse>>(result);
    }
}
