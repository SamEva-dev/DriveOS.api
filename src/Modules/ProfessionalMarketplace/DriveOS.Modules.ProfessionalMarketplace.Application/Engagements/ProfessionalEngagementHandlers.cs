using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;

public sealed class CreateProfessionalEngagementCommandHandler(
    IProfessionalEngagementRepository engagements,
    IProfessionalCommercialOfferRepository offers,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<CreateProfessionalEngagementCommand,ProfessionalEngagementId>
{
    public async Task<Result<ProfessionalEngagementId>> Handle(CreateProfessionalEngagementCommand c,CancellationToken ct)
    {
        if(await engagements.ExistsForCommercialOfferAsync(c.CommercialOfferId,ct))
            return Result.Failure<ProfessionalEngagementId>(ProfessionalEngagementErrors.DuplicateEngagement);

        var offer=await offers.GetAsync(c.CommercialOfferId,false,ct);
        if(offer is null || offer.OrganizationId != c.OrganizationId)
            return Result.Failure<ProfessionalEngagementId>(ProfessionalCommercialOfferErrors.NotFound);

        var created=ProfessionalEngagement.Create(c.Id,c.BranchId,offer,clock.UtcNow,c.ActorUserId);
        if(created.IsFailure)return Result.Failure<ProfessionalEngagementId>(created.Error);

        engagements.Add(created.Value);
        await uow.CommitAsync(ct);
        return Result.Success(created.Value.Id);
    }
}

public abstract class ProfessionalEngagementMutation
{
    protected static async Task<Result> Run(
        ProfessionalEngagementId id,
        OrganizationId organizationId,
        Func<ProfessionalEngagement,Result> mutate,
        IProfessionalEngagementRepository repo,
        IProfessionalMarketplaceUnitOfWork uow,
        CancellationToken ct)
    {
        var x=await repo.GetAsync(id,true,ct);
        if(x is null||x.OrganizationId!=organizationId)
            return Result.Failure(ProfessionalEngagementErrors.NotFound);

        var r=mutate(x);
        if(r.IsFailure)return r;

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class MarkEngagementPreparationCommandHandler(
    IProfessionalEngagementRepository repo,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ProfessionalEngagementMutation,ICommandHandler<MarkEngagementPreparationCommand>
{
    public Task<Result> Handle(MarkEngagementPreparationCommand c,CancellationToken ct)
    {
        if(c.Step==EngagementPreparationStep.Scheduling)
            return Task.FromResult(Result.Failure(ProfessionalEngagementErrors.SchedulingPreparationMustBeValidated));
        if(c.Step==EngagementPreparationStep.Access)
            return Task.FromResult(Result.Failure(DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants.ExternalAccessGrantErrors.AccessPreparationMustBeValidated));
        if(c.Step==EngagementPreparationStep.Contract)
            return Task.FromResult(Result.Failure(ProfessionalEngagementErrors.ContractPreparationMustBeValidated));
        if(c.Step==EngagementPreparationStep.Compliance)
            return Task.FromResult(Result.Failure(ProfessionalEngagementErrors.CompliancePreparationMustBeValidated));

        return Run(c.Id,c.OrganizationId,x=>x.MarkPreparation(c.Step,c.Completed,clock.UtcNow,c.ActorUserId),repo,uow,ct);
    }
}


public sealed class PrepareProfessionalEngagementSchedulingCommandHandler(
    IProfessionalEngagementRepository engagements,
    IProfessionalProfileRepository profiles,
    IProfessionalSchedulingPreparationGateway scheduling,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<PrepareProfessionalEngagementSchedulingCommand,ProfessionalSchedulingPreparationResult>
{
    public async Task<Result<ProfessionalSchedulingPreparationResult>> Handle(
        PrepareProfessionalEngagementSchedulingCommand c,
        CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.Id,true,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalSchedulingPreparationResult>(ProfessionalEngagementErrors.NotFound);

        ProfessionalProfile? profile=await profiles.GetByIdAsync(engagement.ProfessionalProfileId,ct);
        if(profile is null)
            return Result.Failure<ProfessionalSchedulingPreparationResult>(ProfessionalProfileErrors.NotFound);

        if(profile.UserId is not UserId professionalUserId||professionalUserId.IsEmpty)
            return Result.Failure<ProfessionalSchedulingPreparationResult>(ProfessionalEngagementErrors.ProfessionalUserRequired);

        if(engagement.TermsSnapshot.TeachingCategoryCodes.Any(category =>
            !profile.TeachingCategoryCodes.Contains(category,StringComparer.Ordinal)))
            return Result.Failure<ProfessionalSchedulingPreparationResult>(ProfessionalEngagementErrors.SchedulingCategoryMismatch);

        string? timeZoneId=profile.AvailabilityPolicy.RecurringRules
            .Select(x=>x.TimeZoneId)
            .FirstOrDefault(x=>!string.IsNullOrWhiteSpace(x));

        if(string.IsNullOrWhiteSpace(timeZoneId))
            return Result.Failure<ProfessionalSchedulingPreparationResult>(ProfessionalEngagementErrors.SchedulingTimeZoneRequired);

        string displayName=profile.Headline??profile.TradeName??profile.LegalName??"Professional";

        ProfessionalSchedulingPreparationResult prepared=await scheduling.PrepareAsync(
            new ProfessionalSchedulingPreparationRequest(
                engagement.OrganizationId,
                engagement.BranchId,
                professionalUserId,
                displayName,
                timeZoneId,
                engagement.TermsSnapshot.TeachingCategoryCodes,
                engagement.StartsOn,
                engagement.EndsOn),
            ct);

        if(!prepared.IsPrepared)
        {
            engagement.MarkPreparation(EngagementPreparationStep.Scheduling,false,clock.UtcNow,c.ActorUserId);
            await uow.CommitAsync(ct);
            return Result.Success(prepared);
        }

        Result marked=engagement.MarkPreparation(
            EngagementPreparationStep.Scheduling,
            true,
            clock.UtcNow,
            c.ActorUserId);

        if(marked.IsFailure)
            return Result.Failure<ProfessionalSchedulingPreparationResult>(marked.Error);

        await uow.CommitAsync(ct);
        return Result.Success(prepared);
    }
}

public sealed class ActivateProfessionalEngagementCommandHandler(
    IProfessionalEngagementRepository repo,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ProfessionalEngagementMutation,ICommandHandler<ActivateProfessionalEngagementCommand>
{
    public Task<Result> Handle(ActivateProfessionalEngagementCommand c,CancellationToken ct)=>
        Run(c.Id,c.OrganizationId,x=>x.Activate(DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),clock.UtcNow,c.ActorUserId),repo,uow,ct);
}

public sealed class SuspendProfessionalEngagementCommandHandler(
    IProfessionalEngagementRepository repo,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ProfessionalEngagementMutation,ICommandHandler<SuspendProfessionalEngagementCommand>
{
    public Task<Result> Handle(SuspendProfessionalEngagementCommand c,CancellationToken ct)=>
        Run(c.Id,c.OrganizationId,x=>x.Suspend(c.Reason,clock.UtcNow,c.ActorUserId),repo,uow,ct);
}

public sealed class ResumeProfessionalEngagementCommandHandler(
    IProfessionalEngagementRepository repo,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ProfessionalEngagementMutation,ICommandHandler<ResumeProfessionalEngagementCommand>
{
    public Task<Result> Handle(ResumeProfessionalEngagementCommand c,CancellationToken ct)=>
        Run(c.Id,c.OrganizationId,x=>x.Resume(DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),clock.UtcNow,c.ActorUserId),repo,uow,ct);
}

public sealed class CompleteProfessionalEngagementCommandHandler(
    IProfessionalEngagementRepository engagements,
    ProfessionalEngagementClosureService closure)
    :ICommandHandler<CompleteProfessionalEngagementCommand,ProfessionalEngagementClosureResult>
{
    public async Task<Result<ProfessionalEngagementClosureResult>> Handle(
        CompleteProfessionalEngagementCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.Id,true,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalEngagementClosureResult>(ProfessionalEngagementErrors.NotFound);

        return await closure.CloseAsync(
            engagement,
            ProfessionalEngagementClosureMode.Completed,
            "Engagement completed",
            c.ActorUserId,
            ct);
    }
}

public sealed class TerminateProfessionalEngagementCommandHandler(
    IProfessionalEngagementRepository engagements,
    ProfessionalEngagementClosureService closure)
    :ICommandHandler<TerminateProfessionalEngagementCommand,ProfessionalEngagementClosureResult>
{
    public async Task<Result<ProfessionalEngagementClosureResult>> Handle(
        TerminateProfessionalEngagementCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.Id,true,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalEngagementClosureResult>(ProfessionalEngagementErrors.NotFound);

        return await closure.CloseAsync(
            engagement,
            ProfessionalEngagementClosureMode.Terminated,
            c.Reason,
            c.ActorUserId,
            ct);
    }
}


public sealed class ListProfessionalEngagementsQueryHandler(
    IProfessionalEngagementRepository engagements,
    IProfessionalServiceContractGateway contracts)
    :IQueryHandler<ListProfessionalEngagementsQuery,IReadOnlyList<ProfessionalEngagementView>>
{
    public async Task<Result<IReadOnlyList<ProfessionalEngagementView>>> Handle(ListProfessionalEngagementsQuery q,CancellationToken ct)
    {
        IReadOnlyList<ProfessionalEngagement> items=await engagements.ListByProfileAsync(q.ProfessionalProfileId,ct);
        var filtered=items.Where(x=>x.OrganizationId==q.OrganizationId && (q.CommercialOfferId is null||x.CommercialOfferId==q.CommercialOfferId)).ToArray();
        var result=new List<ProfessionalEngagementView>(filtered.Length);
        foreach(var x in filtered)
            result.Add(await MapAsync(x,contracts,ct));
        return Result.Success<IReadOnlyList<ProfessionalEngagementView>>(result);
    }

    internal static async Task<ProfessionalEngagementView> MapAsync(ProfessionalEngagement x,IProfessionalServiceContractGateway contracts,CancellationToken ct)
    {
        ProfessionalServiceContractSnapshot? contract=await contracts.GetByEngagementAsync(x.Id,ct);
        var t=x.TermsSnapshot;
        return new ProfessionalEngagementView(
            x.Id.Value,x.OrganizationId.Value,x.BranchId?.Value,x.ProfessionalProfileId.Value,x.CommercialOfferId.Value,x.CommercialOfferRevision,
            new ProfessionalEngagementTermsView(t.StartsOn,t.EndsOn,t.TeachingCategoryCodes,t.EngagementType.ToString(),t.VehicleProvisionMode.ToString(),t.EstimatedMinutes,t.RateAmount,t.Currency,t.RateUnit?.ToString(),t.MileageRate,t.VehicleAllowance,t.MinimumGuaranteedAmount,t.ClauseCodes),
            x.Status.ToString(),x.CompliancePrepared,x.ContractPrepared,x.AccessPrepared,x.SchedulingPrepared,x.InternalApprovalPrepared,x.IsOperationallyReady,
            x.ActivatedAtUtc,x.SuspendedAtUtc,x.EndedAtUtc,x.InitialIntegrationCompletedAtUtc,x.StatusReason,contract);
    }
}

public sealed class GetProfessionalEngagementQueryHandler(
    IProfessionalEngagementRepository engagements,
    IProfessionalServiceContractGateway contracts)
    :IQueryHandler<GetProfessionalEngagementQuery,ProfessionalEngagementView>
{
    public async Task<Result<ProfessionalEngagementView>> Handle(GetProfessionalEngagementQuery q,CancellationToken ct)
    {
        ProfessionalEngagement? x=await engagements.GetAsync(q.EngagementId,false,ct);
        if(x is null||x.OrganizationId!=q.OrganizationId)
            return Result.Failure<ProfessionalEngagementView>(ProfessionalEngagementErrors.NotFound);
        return Result.Success(await ListProfessionalEngagementsQueryHandler.MapAsync(x,contracts,ct));
    }
}
