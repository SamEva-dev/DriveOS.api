using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Application.Notifications;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.ServiceEntries;

public sealed class RecordServiceEntryCommandHandler(
    IServiceEntryRepository entries,IProfessionalEngagementRepository engagements,IProfessionalMissionRepository missions,
    IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<RecordServiceEntryCommand,ServiceEntryId>
{
    public async Task<Result<ServiceEntryId>> Handle(RecordServiceEntryCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? e=await engagements.GetAsync(c.EngagementId,false,ct);
        if(e is null||e.OrganizationId!=c.OrganizationId)
            return Result.Failure<ServiceEntryId>(ProfessionalEngagementErrors.NotFound);
        if(e.Status is not ProfessionalEngagementStatus.Active and not ProfessionalEngagementStatus.Ended)
            return Result.Failure<ServiceEntryId>(ServiceEntryErrors.ActiveEngagementRequired);

        if(c.MissionId is ProfessionalMissionId mid)
        {
            ProfessionalMission? m=await missions.GetAsync(mid,false,ct);
            if(m is null||m.EngagementId!=e.Id||m.OrganizationId!=e.OrganizationId)
                return Result.Failure<ServiceEntryId>(ProfessionalMissionErrors.NotFound);
        }

        if(await entries.ExistsForSourceAsync(e.Id,c.SourceType,c.SourceId,ct))
            return Result.Failure<ServiceEntryId>(ServiceEntryErrors.DuplicateSource);

        var created=ServiceEntry.Create(c.Id,e.Id,c.MissionId,e.ProfessionalProfileId,e.OrganizationId,e.BranchId,
            c.SourceType,c.SourceId,c.ServiceDate,c.ServiceCode,c.QuantityMinutes,c.UnitRate,
            c.ExpensesAmount,c.IndemnitiesAmount,c.DiscountAmount,c.Currency,c.Description,
            e.StartsOn,e.EndsOn,clock.UtcNow,c.ActorUserId);
        if(created.IsFailure)return Result.Failure<ServiceEntryId>(created.Error);

        entries.Add(created.Value);await uow.CommitAsync(ct);return Result.Success(created.Value.Id);
    }
}

public abstract class ServiceEntryMutation
{
    protected static async Task<Result> Organization(ServiceEntryId id,OrganizationId org,Func<ServiceEntry,Result> action,
        IServiceEntryRepository repo,IProfessionalMarketplaceUnitOfWork uow,CancellationToken ct)
    {
        var x=await repo.GetAsync(id,true,ct);
        if(x is null||x.OrganizationId!=org)return Result.Failure(ServiceEntryErrors.NotFound);
        var r=action(x);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
    protected static async Task<Result> Professional(ServiceEntryId id,ProfessionalProfileId profile,Func<ServiceEntry,Result> action,
        IServiceEntryRepository repo,IProfessionalMarketplaceUnitOfWork uow,CancellationToken ct)
    {
        var x=await repo.GetAsync(id,true,ct);
        if(x is null||x.ProfessionalProfileId!=profile)return Result.Failure(ServiceEntryErrors.NotFound);
        var r=action(x);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}
public sealed class SubmitServiceEntryCommandHandler(IServiceEntryRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ServiceEntryMutation,ICommandHandler<SubmitServiceEntryCommand>
{
    public Task<Result> Handle(SubmitServiceEntryCommand c,CancellationToken ct)=>Professional(c.Id,c.ProfileId,x=>x.Submit(clock.UtcNow,c.ActorUserId),repo,uow,ct);
}
public sealed class ApproveServiceEntryCommandHandler(IServiceEntryRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ServiceEntryMutation,ICommandHandler<ApproveServiceEntryCommand>
{
    public Task<Result> Handle(ApproveServiceEntryCommand c,CancellationToken ct)=>Organization(c.Id,c.OrganizationId,x=>x.Approve(clock.UtcNow,c.ActorUserId),repo,uow,ct);
}
public sealed class RejectServiceEntryCommandHandler(
    IServiceEntryRepository repo,IProfessionalProfileRepository profiles,IMarketplaceNotificationGateway notifications,
    IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<RejectServiceEntryCommand>
{
    public async Task<Result> Handle(RejectServiceEntryCommand c,CancellationToken ct)
    {
        ServiceEntry? entry=await repo.GetAsync(c.Id,true,ct);
        if(entry is null||entry.OrganizationId!=c.OrganizationId)return Result.Failure(ServiceEntryErrors.NotFound);
        Result r=entry.Reject(c.Reason,clock.UtcNow,c.ActorUserId);if(r.IsFailure)return r;
        await uow.CommitAsync(ct);
        ProfessionalProfile? profile=await profiles.GetByIdAsync(entry.ProfessionalProfileId,ct);
        if(profile is not null&&profile.UserId is UserId professionalUserId&&!professionalUserId.IsEmpty)
            await notifications.TryEnqueueAsync(new("User",professionalUserId.Value,entry.OrganizationId,"SERVICE_ENTRY",
                "professionalMarketplace.notifications.serviceEntryRejected",$"service-entry-rejected:{entry.Id.Value}",
                new Dictionary<string,string?>{{"serviceEntryId",entry.Id.Value.ToString()},{"reason",entry.ReviewReason}},
                "SERVICE_ENTRY",entry.Id.Value,
                profile.ProfessionalEmail,
                profile.Languages.FirstOrDefault(x=>x.StartsWith("fr",StringComparison.OrdinalIgnoreCase))??"en",
                c.ActorUserId),ct);
        return Result.Success();
    }
}
public sealed class DisputeServiceEntryCommandHandler(
    IServiceEntryRepository repo,IProfessionalProfileRepository profiles,IMarketplaceNotificationGateway notifications,
    IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<DisputeServiceEntryCommand>
{
    public async Task<Result> Handle(DisputeServiceEntryCommand c,CancellationToken ct)
    {
        ServiceEntry? entry=await repo.GetAsync(c.Id,true,ct);
        if(entry is null||entry.OrganizationId!=c.OrganizationId)return Result.Failure(ServiceEntryErrors.NotFound);
        Result r=entry.OpenDispute(c.Reason,clock.UtcNow,c.ActorUserId);if(r.IsFailure)return r;
        await uow.CommitAsync(ct);
        ProfessionalProfile? profile=await profiles.GetByIdAsync(entry.ProfessionalProfileId,ct);
        if(profile is not null&&profile.UserId is UserId professionalUserId&&!professionalUserId.IsEmpty)
            await notifications.TryEnqueueAsync(new("User",professionalUserId.Value,entry.OrganizationId,"SERVICE_ENTRY",
                "professionalMarketplace.notifications.serviceEntryDisputed",$"service-entry-disputed:{entry.Id.Value}",
                new Dictionary<string,string?>{{"serviceEntryId",entry.Id.Value.ToString()},{"reason",entry.ReviewReason}},
                "SERVICE_ENTRY",entry.Id.Value,
                profile.ProfessionalEmail,
                profile.Languages.FirstOrDefault(x=>x.StartsWith("fr",StringComparison.OrdinalIgnoreCase))??"en",
                c.ActorUserId),ct);
        return Result.Success();
    }
}

public sealed class ListCurrentProfessionalServiceEntriesQueryHandler(
    IServiceEntryRepository entries,
    IProfessionalProfileRepository profiles,
    IProfessionalMissionRepository missions)
    :IQueryHandler<ListCurrentProfessionalServiceEntriesQuery,IReadOnlyList<ServiceEntryResponse>>
{
    public async Task<Result<IReadOnlyList<ServiceEntryResponse>>> Handle(ListCurrentProfessionalServiceEntriesQuery q,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(q.UserId,ct);
        if(profile is null)return Result.Failure<IReadOnlyList<ServiceEntryResponse>>(ProfessionalProfileErrors.NotFound);

        IReadOnlyList<ServiceEntry> source;
        if(q.MissionId is ProfessionalMissionId missionId)
        {
            ProfessionalMission? mission=await missions.GetAsync(missionId,false,ct);
            if(mission is null||mission.ProfessionalProfileId!=profile.Id)
                return Result.Failure<IReadOnlyList<ServiceEntryResponse>>(ProfessionalMissionErrors.NotFound);
            source=await entries.ListByMissionAsync(missionId,ct);
            source=source.Where(x=>x.ProfessionalProfileId==profile.Id).ToArray();
        }
        else source=await entries.ListByProfileAsync(profile.Id,ct);

        return Result.Success<IReadOnlyList<ServiceEntryResponse>>(source.Select(Map).ToArray());
    }

    internal static ServiceEntryResponse Map(ServiceEntry x)=>new(
        x.Id.Value,x.EngagementId.Value,x.MissionId?.Value,x.ProfessionalProfileId.Value,x.OrganizationId.Value,x.BranchId?.Value,
        x.SourceType.ToString(),x.SourceId,x.ServiceDate,x.ServiceCode,x.QuantityMinutes,x.UnitRate,x.BaseAmount,x.ExpensesAmount,
        x.IndemnitiesAmount,x.DiscountAmount,x.TotalAmount,x.Currency,x.Description,x.Status.ToString(),x.SubmittedAtUtc,x.ReviewedAtUtc,
        x.ReviewedByUserId?.Value,x.ReviewReason,x.CreatedAtUtc);
}

public sealed class GetCurrentProfessionalServiceEntryQueryHandler(
    IServiceEntryRepository entries,IProfessionalProfileRepository profiles)
    :IQueryHandler<GetCurrentProfessionalServiceEntryQuery,ServiceEntryResponse>
{
    public async Task<Result<ServiceEntryResponse>> Handle(GetCurrentProfessionalServiceEntryQuery q,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(q.UserId,ct);
        if(profile is null)return Result.Failure<ServiceEntryResponse>(ProfessionalProfileErrors.NotFound);
        ServiceEntry? entry=await entries.GetAsync(q.Id,false,ct);
        if(entry is null||entry.ProfessionalProfileId!=profile.Id)return Result.Failure<ServiceEntryResponse>(ServiceEntryErrors.NotFound);
        return Result.Success(ListCurrentProfessionalServiceEntriesQueryHandler.Map(entry));
    }
}

public sealed class SubmitCurrentProfessionalServiceEntryCommandHandler(
    IServiceEntryRepository entries,IProfessionalProfileRepository profiles,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<SubmitCurrentProfessionalServiceEntryCommand>
{
    public async Task<Result> Handle(SubmitCurrentProfessionalServiceEntryCommand q,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(q.UserId,ct);
        if(profile is null)return Result.Failure(ProfessionalProfileErrors.NotFound);
        ServiceEntry? entry=await entries.GetAsync(q.Id,true,ct);
        if(entry is null||entry.ProfessionalProfileId!=profile.Id)return Result.Failure(ServiceEntryErrors.NotFound);
        Result result=entry.Submit(clock.UtcNow,q.UserId);
        if(result.IsFailure)return result;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
