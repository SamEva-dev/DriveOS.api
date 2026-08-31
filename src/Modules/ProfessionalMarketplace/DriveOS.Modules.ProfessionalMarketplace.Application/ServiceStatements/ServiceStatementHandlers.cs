using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceStatements;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.ServiceStatements;

public sealed class CreateServiceStatementCommandHandler(
    IServiceStatementRepository statements,IServiceEntryRepository entries,
    IProfessionalEngagementRepository engagements,IProfessionalProfileRepository profiles,
    IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<CreateServiceStatementCommand,ServiceStatementId>
{
    public async Task<Result<ServiceStatementId>> Handle(CreateServiceStatementCommand c,CancellationToken ct)
    {
        var engagement=await engagements.GetAsync(c.EngagementId,false,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ServiceStatementId>(ProfessionalEngagementErrors.NotFound);

        if(await statements.ExistsForPeriodAsync(c.EngagementId,c.PeriodStart,c.PeriodEnd,ct))
            return Result.Failure<ServiceStatementId>(ServiceStatementErrors.DuplicatePeriod);

        var profile=await profiles.GetByIdAsync(engagement.ProfessionalProfileId,ct);
        if(profile is null)return Result.Failure<ServiceStatementId>(ProfessionalProfileErrors.NotFound);

        IReadOnlyList<ServiceEntry> all=await entries.ListByEngagementAsync(engagement.Id,ct);
        var selected=all.Where(x=>x.ServiceDate>=c.PeriodStart&&x.ServiceDate<=c.PeriodEnd).ToArray();

        var created=ServiceStatement.Create(c.Id,engagement.Id,engagement.ProfessionalProfileId,
            engagement.OrganizationId,profile.ProviderOrganizationId.Value,c.PeriodStart,c.PeriodEnd,
            selected,clock.UtcNow,c.ActorUserId);
        if(created.IsFailure)return Result.Failure<ServiceStatementId>(created.Error);

        statements.Add(created.Value);await uow.CommitAsync(ct);return Result.Success(created.Value.Id);
    }
}

public abstract class ServiceStatementMutation
{
    protected static async Task<Result> Organization(ServiceStatementId id,OrganizationId org,Func<ServiceStatement,Result> action,
        IServiceStatementRepository repo,IProfessionalMarketplaceUnitOfWork uow,CancellationToken ct)
    {
        var x=await repo.GetAsync(id,true,ct);
        if(x is null||x.ClientOrganizationId!=org)return Result.Failure(ServiceStatementErrors.NotFound);
        var r=action(x);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
    protected static async Task<Result> Professional(ServiceStatementId id,ProfessionalProfileId profile,Func<ServiceStatement,Result> action,
        IServiceStatementRepository repo,IProfessionalMarketplaceUnitOfWork uow,CancellationToken ct)
    {
        var x=await repo.GetAsync(id,true,ct);
        if(x is null||x.ProfessionalProfileId!=profile)return Result.Failure(ServiceStatementErrors.NotFound);
        var r=action(x);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}

public sealed class SubmitServiceStatementCommandHandler(IServiceStatementRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ServiceStatementMutation,ICommandHandler<SubmitServiceStatementCommand>
{
    public Task<Result> Handle(SubmitServiceStatementCommand c,CancellationToken ct)=>Professional(c.Id,c.ProfileId,x=>x.Submit(clock.UtcNow,c.ActorUserId),repo,uow,ct);
}
public sealed class StartServiceStatementReviewCommandHandler(IServiceStatementRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ServiceStatementMutation,ICommandHandler<StartServiceStatementReviewCommand>
{
    public Task<Result> Handle(StartServiceStatementReviewCommand c,CancellationToken ct)=>Organization(c.Id,c.OrganizationId,x=>x.StartReview(clock.UtcNow,c.ActorUserId),repo,uow,ct);
}
public sealed class RefreshServiceStatementCommandHandler(
    IServiceStatementRepository statements,IServiceEntryRepository entries,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<RefreshServiceStatementCommand>
{
    public async Task<Result> Handle(RefreshServiceStatementCommand c,CancellationToken ct)
    {
        var statement=await statements.GetAsync(c.Id,true,ct);
        if(statement is null||statement.ClientOrganizationId!=c.OrganizationId)return Result.Failure(ServiceStatementErrors.NotFound);
        var lines=await entries.ListByEngagementAsync(statement.EngagementId,ct);
        var r=statement.Refresh(lines,clock.UtcNow,c.ActorUserId);
        if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}
public sealed class ApproveServiceStatementCommandHandler(
    IServiceStatementRepository statements,IServiceEntryRepository entries,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<ApproveServiceStatementCommand>
{
    public async Task<Result> Handle(ApproveServiceStatementCommand c,CancellationToken ct)
    {
        var statement=await statements.GetAsync(c.Id,true,ct);
        if(statement is null||statement.ClientOrganizationId!=c.OrganizationId)return Result.Failure(ServiceStatementErrors.NotFound);

        var all=await entries.ListByEngagementAsync(statement.EngagementId,ct);
        var ids=statement.Lines.Select(x=>x.ServiceEntryId).ToHashSet();
        foreach(var line in all.Where(x=>ids.Contains(x.Id)))
        {
            if(line.Status==ServiceEntryStatus.Submitted)
            {
                var tracked=await entries.GetAsync(line.Id,true,ct);
                if(tracked is not null)
                {
                    var approved=tracked.Approve(clock.UtcNow,c.ActorUserId);
                    if(approved.IsFailure)return approved;
                }
            }
        }
        var refreshed=await entries.ListByEngagementAsync(statement.EngagementId,ct);
        var r=statement.Refresh(refreshed,clock.UtcNow,c.ActorUserId);
        if(r.IsFailure)return r;
        await uow.CommitAsync(ct);return Result.Success();
    }
}
public sealed class RejectServiceStatementCommandHandler(IServiceStatementRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ServiceStatementMutation,ICommandHandler<RejectServiceStatementCommand>
{
    public Task<Result> Handle(RejectServiceStatementCommand c,CancellationToken ct)=>Organization(c.Id,c.OrganizationId,x=>x.Reject(c.Reason,clock.UtcNow,c.ActorUserId),repo,uow,ct);
}


public sealed class ListOrganizationServiceStatementsQueryHandler(
    IServiceStatementRepository statements,IProfessionalEngagementRepository engagements)
    :IQueryHandler<ListOrganizationServiceStatementsQuery,IReadOnlyList<ServiceStatementResponse>>
{
    public async Task<Result<IReadOnlyList<ServiceStatementResponse>>> Handle(ListOrganizationServiceStatementsQuery q,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(q.EngagementId,false,ct);
        if(engagement is null||engagement.OrganizationId!=q.OrganizationId)
            return Result.Failure<IReadOnlyList<ServiceStatementResponse>>(ProfessionalEngagementErrors.NotFound);
        IReadOnlyList<ServiceStatement> source=await statements.ListByEngagementAsync(q.EngagementId,ct);
        return Result.Success<IReadOnlyList<ServiceStatementResponse>>(source.Select(ListCurrentProfessionalServiceStatementsQueryHandler.Map).ToArray());
    }
}

public sealed class GetOrganizationServiceStatementQueryHandler(IServiceStatementRepository statements)
    :IQueryHandler<GetOrganizationServiceStatementQuery,ServiceStatementResponse>
{
    public async Task<Result<ServiceStatementResponse>> Handle(GetOrganizationServiceStatementQuery q,CancellationToken ct)
    {
        ServiceStatement? statement=await statements.GetAsync(q.Id,false,ct);
        if(statement is null||statement.ClientOrganizationId!=q.OrganizationId)
            return Result.Failure<ServiceStatementResponse>(ServiceStatementErrors.NotFound);
        return Result.Success(ListCurrentProfessionalServiceStatementsQueryHandler.Map(statement));
    }
}

public sealed class ListCurrentProfessionalServiceStatementsQueryHandler(
    IServiceStatementRepository statements,IProfessionalProfileRepository profiles,IProfessionalEngagementRepository engagements)
    :IQueryHandler<ListCurrentProfessionalServiceStatementsQuery,IReadOnlyList<ServiceStatementResponse>>
{
    public async Task<Result<IReadOnlyList<ServiceStatementResponse>>> Handle(ListCurrentProfessionalServiceStatementsQuery q,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(q.UserId,ct);
        if(profile is null)return Result.Failure<IReadOnlyList<ServiceStatementResponse>>(ProfessionalProfileErrors.NotFound);
        IReadOnlyList<ServiceStatement> source;
        if(q.EngagementId is ProfessionalEngagementId engagementId)
        {
            ProfessionalEngagement? engagement=await engagements.GetAsync(engagementId,false,ct);
            if(engagement is null||engagement.ProfessionalProfileId!=profile.Id)
                return Result.Failure<IReadOnlyList<ServiceStatementResponse>>(ProfessionalEngagementErrors.NotFound);
            source=await statements.ListByEngagementAsync(engagementId,ct);
        }
        else source=await statements.ListByProfileAsync(profile.Id,ct);
        return Result.Success<IReadOnlyList<ServiceStatementResponse>>(source.Select(Map).ToArray());
    }

    internal static ServiceStatementResponse Map(ServiceStatement x)=>new(
        x.Id.Value,x.EngagementId.Value,x.ProfessionalProfileId.Value,x.ClientOrganizationId.Value,x.ProviderOrganizationId,
        x.PeriodStart,x.PeriodEnd,x.Currency,x.TotalAmount,x.ApprovedAmount,x.DisputedAmount,x.Status.ToString(),x.SubmittedAtUtc,
        x.ReviewedAtUtc,x.ReviewedByUserId?.Value,x.RejectionReason,x.CreatedAtUtc,
        x.Lines.Select(l=>new ServiceStatementLineResponse(l.ServiceEntryId.Value,l.ServiceDate,l.ServiceCode,l.QuantityMinutes,l.UnitRate,l.Currency,l.TotalAmount,l.Description,l.EntryStatus.ToString())).ToArray());
}

public sealed class GetCurrentProfessionalServiceStatementQueryHandler(IServiceStatementRepository statements,IProfessionalProfileRepository profiles)
    :IQueryHandler<GetCurrentProfessionalServiceStatementQuery,ServiceStatementResponse>
{
    public async Task<Result<ServiceStatementResponse>> Handle(GetCurrentProfessionalServiceStatementQuery q,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(q.UserId,ct);
        if(profile is null)return Result.Failure<ServiceStatementResponse>(ProfessionalProfileErrors.NotFound);
        ServiceStatement? statement=await statements.GetAsync(q.Id,false,ct);
        if(statement is null||statement.ProfessionalProfileId!=profile.Id)return Result.Failure<ServiceStatementResponse>(ServiceStatementErrors.NotFound);
        return Result.Success(ListCurrentProfessionalServiceStatementsQueryHandler.Map(statement));
    }
}

public sealed class CreateCurrentProfessionalServiceStatementCommandHandler(
    IServiceStatementRepository statements,IServiceEntryRepository entries,IProfessionalEngagementRepository engagements,
    IProfessionalProfileRepository profiles,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<CreateCurrentProfessionalServiceStatementCommand,ServiceStatementId>
{
    public async Task<Result<ServiceStatementId>> Handle(CreateCurrentProfessionalServiceStatementCommand c,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(c.UserId,ct);
        if(profile is null)return Result.Failure<ServiceStatementId>(ProfessionalProfileErrors.NotFound);
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.EngagementId,false,ct);
        if(engagement is null||engagement.ProfessionalProfileId!=profile.Id)return Result.Failure<ServiceStatementId>(ProfessionalEngagementErrors.NotFound);
        if(await statements.ExistsForPeriodAsync(engagement.Id,c.PeriodStart,c.PeriodEnd,ct))
            return Result.Failure<ServiceStatementId>(ServiceStatementErrors.DuplicatePeriod);
        IReadOnlyList<ServiceEntry> all=await entries.ListByEngagementAsync(engagement.Id,ct);
        var created=ServiceStatement.Create(new ServiceStatementId(Guid.NewGuid()),engagement.Id,profile.Id,engagement.OrganizationId,
            profile.ProviderOrganizationId.Value,c.PeriodStart,c.PeriodEnd,all,clock.UtcNow,c.UserId);
        if(created.IsFailure)return Result.Failure<ServiceStatementId>(created.Error);
        statements.Add(created.Value);await uow.CommitAsync(ct);return Result.Success(created.Value.Id);
    }
}

public sealed class SubmitCurrentProfessionalServiceStatementCommandHandler(
    IServiceStatementRepository statements,IProfessionalProfileRepository profiles,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<SubmitCurrentProfessionalServiceStatementCommand>
{
    public async Task<Result> Handle(SubmitCurrentProfessionalServiceStatementCommand c,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(c.UserId,ct);
        if(profile is null)return Result.Failure(ProfessionalProfileErrors.NotFound);
        ServiceStatement? statement=await statements.GetAsync(c.Id,true,ct);
        if(statement is null||statement.ProfessionalProfileId!=profile.Id)return Result.Failure(ServiceStatementErrors.NotFound);
        Result result=statement.Submit(clock.UtcNow,c.UserId);if(result.IsFailure)return result;
        await uow.CommitAsync(ct);return Result.Success();
    }
}
