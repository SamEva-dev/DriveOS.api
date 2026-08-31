using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Opportunities;

public sealed class CreateProfessionalOpportunityCommandHandler(
    IProfessionalOpportunityRepository repo,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<CreateProfessionalOpportunityCommand,ProfessionalOpportunityId>
{
    public async Task<Result<ProfessionalOpportunityId>> Handle(CreateProfessionalOpportunityCommand c,CancellationToken ct)
    {
        var created=ProfessionalOpportunity.Create(
            c.Id,c.OrganizationId,c.BranchId,c.Title,c.Description,c.ProfessionalType,c.TeachingCategoryCodes,
            c.RequiredLanguageCodes,c.RequiredSpecializationCodes,c.CountryCode,c.AreaCode,c.AreaDisplayName,
            c.Latitude,c.Longitude,c.RadiusKm,c.StartsOn,c.EndsOn,
            c.TimeWindows.Select(x=>new OpportunityTimeWindow(x.DayOfWeek,x.StartTime,x.EndTime,x.TimeZoneId)),
            c.EstimatedMinutes,c.EngagementType,c.VehicleProvisionMode,c.BudgetMin,c.BudgetMax,c.Currency,c.BudgetUnit,
            c.BudgetNegotiable,clock.UtcNow,c.ActorUserId);
        if(created.IsFailure)return Result.Failure<ProfessionalOpportunityId>(created.Error);
        repo.Add(created.Value);await uow.CommitAsync(ct);return Result.Success(created.Value.Id);
    }
}

public sealed class GetProfessionalOpportunityQueryHandler(IProfessionalOpportunityRepository repo):IQueryHandler<GetProfessionalOpportunityQuery,ProfessionalOpportunityResponse>
{
    public async Task<Result<ProfessionalOpportunityResponse>> Handle(GetProfessionalOpportunityQuery q,CancellationToken ct)
    {
        var x=await repo.GetAsync(q.Id,false,ct);
        if(x is null)return Result.Failure<ProfessionalOpportunityResponse>(ProfessionalOpportunityErrors.NotFound);
        if(q.OrganizationId is OrganizationId org&&x.OrganizationId!=org)
            return Result.Failure<ProfessionalOpportunityResponse>(ProfessionalOpportunityErrors.NotFound);
        return Result.Success(Map(x));
    }

    internal static ProfessionalOpportunityResponse Map(ProfessionalOpportunity x)=>new(
        x.Id.Value,x.OrganizationId.Value,x.BranchId?.Value,x.Status.ToString(),x.Title,x.Description,x.ProfessionalType.ToString(),
        x.TeachingCategoryCodes,x.RequiredLanguageCodes,x.RequiredSpecializationCodes,x.CountryCode,x.AreaCode,x.AreaDisplayName,
        x.Latitude,x.Longitude,x.RadiusKm,x.StartsOn,x.EndsOn,
        x.TimeWindows.Select(w=>new OpportunityTimeWindowInput(w.DayOfWeek,w.StartTime,w.EndTime,w.TimeZoneId)).ToArray(),
        x.EstimatedMinutes,x.EngagementType.ToString(),x.VehicleProvisionMode.ToString(),x.BudgetMin,x.BudgetMax,x.Currency,
        x.BudgetUnit?.ToString(),x.BudgetNegotiable,x.PublishedAtUtc,x.ClosedAtUtc,x.ClosureReason);
}

public sealed class ListProfessionalOpportunitiesQueryHandler(IProfessionalOpportunityRepository repo):IQueryHandler<ListProfessionalOpportunitiesQuery,IReadOnlyList<ProfessionalOpportunityResponse>>
{
    public async Task<Result<IReadOnlyList<ProfessionalOpportunityResponse>>> Handle(ListProfessionalOpportunitiesQuery q,CancellationToken ct)
    {
        var items=await repo.ListForOrganizationAsync(q.OrganizationId,ct);
        return Result.Success<IReadOnlyList<ProfessionalOpportunityResponse>>(items.Select(GetProfessionalOpportunityQueryHandler.Map).ToArray());
    }
}

public abstract class ProfessionalOpportunityMutationHandler
{
    protected static async Task<Result> Run(
        ProfessionalOpportunityId id,OrganizationId organizationId,
        Func<ProfessionalOpportunity,Result> mutate,
        IProfessionalOpportunityRepository repo,IProfessionalMarketplaceUnitOfWork uow,CancellationToken ct)
    {
        var x=await repo.GetAsync(id,true,ct);
        if(x is null||x.OrganizationId!=organizationId)return Result.Failure(ProfessionalOpportunityErrors.NotFound);
        var r=mutate(x);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}

public sealed class PublishProfessionalOpportunityCommandHandler(IProfessionalOpportunityRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ProfessionalOpportunityMutationHandler,ICommandHandler<PublishProfessionalOpportunityCommand>
{
    public Task<Result> Handle(PublishProfessionalOpportunityCommand c,CancellationToken ct)=>Run(c.Id,c.OrganizationId,x=>x.Publish(clock.UtcNow,c.ActorUserId),repo,uow,ct);
}
public sealed class PauseProfessionalOpportunityCommandHandler(IProfessionalOpportunityRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ProfessionalOpportunityMutationHandler,ICommandHandler<PauseProfessionalOpportunityCommand>
{
    public Task<Result> Handle(PauseProfessionalOpportunityCommand c,CancellationToken ct)=>Run(c.Id,c.OrganizationId,x=>x.Pause(clock.UtcNow,c.ActorUserId),repo,uow,ct);
}
public sealed class FillProfessionalOpportunityCommandHandler(IProfessionalOpportunityRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ProfessionalOpportunityMutationHandler,ICommandHandler<FillProfessionalOpportunityCommand>
{
    public Task<Result> Handle(FillProfessionalOpportunityCommand c,CancellationToken ct)=>Run(c.Id,c.OrganizationId,x=>x.Fill(clock.UtcNow,c.ActorUserId),repo,uow,ct);
}
public sealed class CancelProfessionalOpportunityCommandHandler(IProfessionalOpportunityRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ProfessionalOpportunityMutationHandler,ICommandHandler<CancelProfessionalOpportunityCommand>
{
    public Task<Result> Handle(CancelProfessionalOpportunityCommand c,CancellationToken ct)=>Run(c.Id,c.OrganizationId,x=>x.Cancel(c.Reason,clock.UtcNow,c.ActorUserId),repo,uow,ct);
}
