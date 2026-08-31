using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Application.Notifications;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.SharedKernel.Results;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Missions;

public sealed class CreateProfessionalMissionCommandHandler(
    IProfessionalMissionRepository missions,
    IProfessionalEngagementRepository engagements,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock) : ICommandHandler<CreateProfessionalMissionCommand, ProfessionalMissionId>
{
    public async Task<Result<ProfessionalMissionId>> Handle(CreateProfessionalMissionCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.EngagementId,false,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalMissionId>(ProfessionalEngagementErrors.NotFound);

        var created=ProfessionalMission.Create(
            c.Id,
            engagement,
            c.BranchId,
            c.Title,
            c.Description,
            c.StartsOn,
            c.EndsOn,
            c.TeachingCategoryCodes,
            c.EstimatedMinutes,
            c.VehicleProvisionMode,
            c.TimeWindows.Select(x=>new MissionTimeWindow(x.DayOfWeek,x.StartTime,x.EndTime,x.TimeZoneId)),
            clock.UtcNow,
            c.ActorUserId);

        if(created.IsFailure)
            return Result.Failure<ProfessionalMissionId>(created.Error);

        missions.Add(created.Value);
        await uow.CommitAsync(ct);
        return Result.Success(created.Value.Id);
    }
}

public sealed class UpdateProfessionalMissionCommandHandler(
    IProfessionalMissionRepository missions,
    IProfessionalEngagementRepository engagements,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock) : ICommandHandler<UpdateProfessionalMissionCommand>
{
    public async Task<Result> Handle(UpdateProfessionalMissionCommand c,CancellationToken ct)
    {
        ProfessionalMission? mission=await missions.GetAsync(c.Id,true,ct);
        if(mission is null||mission.OrganizationId!=c.OrganizationId)
            return Result.Failure(ProfessionalMissionErrors.NotFound);

        ProfessionalEngagement? engagement=await engagements.GetAsync(mission.EngagementId,false,ct);
        if(engagement is null||engagement.Status!=ProfessionalEngagementStatus.Active)
            return Result.Failure(ProfessionalMissionErrors.ActiveEngagementRequired);

        if(c.StartsOn<engagement.StartsOn||c.EndsOn>engagement.EndsOn)
            return Result.Failure(ProfessionalMissionErrors.OutsideEngagementPeriod);

        if(c.TeachingCategoryCodes.Any(x=>!engagement.TermsSnapshot.TeachingCategoryCodes.Contains(x,StringComparer.Ordinal)))
            return Result.Failure(ProfessionalMissionErrors.InvalidTeachingCategories);

        Result result=mission.UpdateDraft(
            c.Title,c.Description,c.StartsOn,c.EndsOn,c.TeachingCategoryCodes,c.EstimatedMinutes,
            c.VehicleProvisionMode,
            c.TimeWindows.Select(x=>new MissionTimeWindow(x.DayOfWeek,x.StartTime,x.EndTime,x.TimeZoneId)),
            clock.UtcNow,c.ActorUserId);

        if(result.IsFailure)return result;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public abstract class ProfessionalMissionMutation
{
    protected static async Task<Result> ForOrganization(
        ProfessionalMissionId id,
        OrganizationId organizationId,
        Func<ProfessionalMission,Result> mutate,
        IProfessionalMissionRepository repo,
        IProfessionalMarketplaceUnitOfWork uow,
        CancellationToken ct)
    {
        ProfessionalMission? mission=await repo.GetAsync(id,true,ct);
        if(mission is null||mission.OrganizationId!=organizationId)
            return Result.Failure(ProfessionalMissionErrors.NotFound);

        Result result=mutate(mission);
        if(result.IsFailure)return result;

        await uow.CommitAsync(ct);
        return Result.Success();
    }

    protected static async Task<Result> ForProfessional(
        ProfessionalMissionId id,
        ProfessionalProfileId profileId,
        Func<ProfessionalMission,Result> mutate,
        IProfessionalMissionRepository repo,
        IProfessionalMarketplaceUnitOfWork uow,
        CancellationToken ct)
    {
        ProfessionalMission? mission=await repo.GetAsync(id,true,ct);
        if(mission is null||mission.ProfessionalProfileId!=profileId)
            return Result.Failure(ProfessionalMissionErrors.NotFound);

        Result result=mutate(mission);
        if(result.IsFailure)return result;

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class ProposeProfessionalMissionCommandHandler(
    IProfessionalMissionRepository repo,
    IProfessionalProfileRepository profiles,
    IMarketplaceNotificationGateway notifications,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock)
    : ICommandHandler<ProposeProfessionalMissionCommand>
{
    public async Task<Result> Handle(ProposeProfessionalMissionCommand c,CancellationToken ct)
    {
        ProfessionalMission? mission=await repo.GetAsync(c.Id,true,ct);
        if(mission is null||mission.OrganizationId!=c.OrganizationId)
            return Result.Failure(ProfessionalMissionErrors.NotFound);

        Result proposed=mission.Propose(clock.UtcNow,c.ActorUserId);
        if(proposed.IsFailure)return proposed;

        await uow.CommitAsync(ct);

        ProfessionalProfile? profile=await profiles.GetByIdAsync(mission.ProfessionalProfileId,ct);
        if(profile is not null&&profile.UserId.HasValue&&!profile.UserId.Value.IsEmpty)
        {
            await notifications.TryEnqueueAsync(new(
                "User",profile.UserId.Value.Value,mission.OrganizationId,"MISSION",
                "professionalMarketplace.notifications.missionProposd",
                $"mission-proposed:{mission.Id.Value}",
                new Dictionary<string,string?>
                {
                    ["missionId"]=mission.Id.Value.ToString(),
                    ["title"]=mission.Title,
                    ["startsOn"]=mission.StartsOn.ToString("yyyy-MM-dd"),
                    ["endsOn"]=mission.EndsOn.ToString("yyyy-MM-dd")
                },
                "PROFESSIONAL_MISSION",mission.Id.Value,
                profile.ProfessionalEmail,
                profile.Languages.FirstOrDefault(x=>x.StartsWith("fr",StringComparison.OrdinalIgnoreCase))??"en",
                c.ActorUserId),ct);
        }

        return Result.Success();
    }
}

public sealed class AcceptProfessionalMissionCommandHandler(
    IProfessionalMissionRepository repo,IProfessionalProfileRepository profiles,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    : ProfessionalMissionMutation,ICommandHandler<AcceptProfessionalMissionCommand>
{
    public async Task<Result> Handle(AcceptProfessionalMissionCommand c,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.GetByIdAsync(c.ProfileId,ct);
        if(profile is null||profile.UserId!=c.ActorUserId)
            return Result.Failure(ProfessionalMissionErrors.NotFound);
        return await ForProfessional(c.Id,c.ProfileId,x=>x.Accept(clock.UtcNow,c.ActorUserId),repo,uow,ct);
    }
}

public sealed class DeclineProfessionalMissionCommandHandler(
    IProfessionalMissionRepository repo,IProfessionalProfileRepository profiles,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    : ProfessionalMissionMutation,ICommandHandler<DeclineProfessionalMissionCommand>
{
    public async Task<Result> Handle(DeclineProfessionalMissionCommand c,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.GetByIdAsync(c.ProfileId,ct);
        if(profile is null||profile.UserId!=c.ActorUserId)
            return Result.Failure(ProfessionalMissionErrors.NotFound);
        return await ForProfessional(c.Id,c.ProfileId,x=>x.Decline(c.Reason,clock.UtcNow,c.ActorUserId),repo,uow,ct);
    }
}

public sealed class ActivateProfessionalMissionCommandHandler(
    IProfessionalMissionRepository missions,
    IProfessionalEngagementRepository engagements,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock) : ICommandHandler<ActivateProfessionalMissionCommand>
{
    public async Task<Result> Handle(ActivateProfessionalMissionCommand c,CancellationToken ct)
    {
        ProfessionalMission? mission=await missions.GetAsync(c.Id,true,ct);
        if(mission is null||mission.OrganizationId!=c.OrganizationId)
            return Result.Failure(ProfessionalMissionErrors.NotFound);

        ProfessionalEngagement? engagement=await engagements.GetAsync(mission.EngagementId,false,ct);
        if(engagement is null||engagement.Status!=ProfessionalEngagementStatus.Active||!engagement.IsOperationallyReady)
            return Result.Failure(ProfessionalMissionErrors.ActiveEngagementRequired);

        Result result=mission.Activate(DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),clock.UtcNow,c.ActorUserId);
        if(result.IsFailure)return result;

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class PauseProfessionalMissionCommandHandler(
    IProfessionalMissionRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    : ProfessionalMissionMutation,ICommandHandler<PauseProfessionalMissionCommand>
{
    public Task<Result> Handle(PauseProfessionalMissionCommand c,CancellationToken ct)=>
        ForOrganization(c.Id,c.OrganizationId,x=>x.Pause(c.Reason,clock.UtcNow,c.ActorUserId),repo,uow,ct);
}

public sealed class ResumeProfessionalMissionCommandHandler(
    IProfessionalMissionRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    : ProfessionalMissionMutation,ICommandHandler<ResumeProfessionalMissionCommand>
{
    public Task<Result> Handle(ResumeProfessionalMissionCommand c,CancellationToken ct)=>
        ForOrganization(c.Id,c.OrganizationId,x=>x.Resume(DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),clock.UtcNow,c.ActorUserId),repo,uow,ct);
}

public sealed class CompleteProfessionalMissionCommandHandler(
    IProfessionalMissionRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    : ProfessionalMissionMutation,ICommandHandler<CompleteProfessionalMissionCommand>
{
    public Task<Result> Handle(CompleteProfessionalMissionCommand c,CancellationToken ct)=>
        ForOrganization(c.Id,c.OrganizationId,x=>x.Complete(DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),clock.UtcNow,c.ActorUserId),repo,uow,ct);
}

public sealed class CancelProfessionalMissionCommandHandler(
    IProfessionalMissionRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    : ProfessionalMissionMutation,ICommandHandler<CancelProfessionalMissionCommand>
{
    public Task<Result> Handle(CancelProfessionalMissionCommand c,CancellationToken ct)=>
        ForOrganization(c.Id,c.OrganizationId,x=>x.Cancel(c.Reason,clock.UtcNow,c.ActorUserId),repo,uow,ct);
}

public sealed class ListProfessionalMissionsQueryHandler(
    IProfessionalMissionRepository repo,
    IProfessionalEngagementRepository engagements) : IQueryHandler<ListProfessionalMissionsQuery,IReadOnlyList<ProfessionalMissionResponse>>
{
    public async Task<Result<IReadOnlyList<ProfessionalMissionResponse>>> Handle(ListProfessionalMissionsQuery q,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(q.EngagementId,false,ct);
        if(engagement is null||engagement.OrganizationId!=q.OrganizationId)
            return Result.Failure<IReadOnlyList<ProfessionalMissionResponse>>(ProfessionalEngagementErrors.NotFound);

        IReadOnlyList<ProfessionalMission> missions=await repo.ListByEngagementAsync(q.EngagementId,ct);
        return Result.Success<IReadOnlyList<ProfessionalMissionResponse>>(missions.Select(ToResponse).ToArray());
    }

    private static ProfessionalMissionResponse ToResponse(ProfessionalMission x)=>new(
        x.Id.Value,x.EngagementId.Value,x.OrganizationId.Value,x.ProfessionalProfileId.Value,x.BranchId?.Value,
        x.Title,x.Description,x.StartsOn,x.EndsOn,x.TeachingCategoryCodes,x.EstimatedMinutes,x.VehicleProvisionMode.ToString(),
        x.TimeWindows.Select(w=>new MissionTimeWindowInput(w.DayOfWeek,w.StartTime,w.EndTime,w.TimeZoneId)).ToArray(),
        x.Status.ToString(),x.ProposedAtUtc,x.RespondedAtUtc,x.ActivatedAtUtc,x.CompletedAtUtc,x.CancelledAtUtc,x.StatusReason);
}

public sealed class ListCurrentProfessionalMissionsQueryHandler(
    IProfessionalMissionRepository missions,
    IProfessionalProfileRepository profiles) : IQueryHandler<ListCurrentProfessionalMissionsQuery,IReadOnlyList<ProfessionalMissionResponse>>
{
    public async Task<Result<IReadOnlyList<ProfessionalMissionResponse>>> Handle(ListCurrentProfessionalMissionsQuery q,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(q.UserId,ct);
        if(profile is null)
            return Result.Failure<IReadOnlyList<ProfessionalMissionResponse>>(ProfessionalProfileErrors.NotFound);

        IReadOnlyList<ProfessionalMission> items=await missions.ListByProfileAsync(profile.Id,ct);
        return Result.Success<IReadOnlyList<ProfessionalMissionResponse>>(items.Select(ToResponse).ToArray());
    }

    private static ProfessionalMissionResponse ToResponse(ProfessionalMission x)=>new(
        x.Id.Value,x.EngagementId.Value,x.OrganizationId.Value,x.ProfessionalProfileId.Value,x.BranchId?.Value,
        x.Title,x.Description,x.StartsOn,x.EndsOn,x.TeachingCategoryCodes,x.EstimatedMinutes,x.VehicleProvisionMode.ToString(),
        x.TimeWindows.Select(w=>new MissionTimeWindowInput(w.DayOfWeek,w.StartTime,w.EndTime,w.TimeZoneId)).ToArray(),
        x.Status.ToString(),x.ProposedAtUtc,x.RespondedAtUtc,x.ActivatedAtUtc,x.CompletedAtUtc,x.CancelledAtUtc,x.StatusReason);
}

public sealed class GetCurrentProfessionalMissionQueryHandler(
    IProfessionalMissionRepository missions,
    IProfessionalProfileRepository profiles) : IQueryHandler<GetCurrentProfessionalMissionQuery,ProfessionalMissionResponse>
{
    public async Task<Result<ProfessionalMissionResponse>> Handle(GetCurrentProfessionalMissionQuery q,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(q.UserId,ct);
        if(profile is null)
            return Result.Failure<ProfessionalMissionResponse>(ProfessionalProfileErrors.NotFound);

        ProfessionalMission? x=await missions.GetAsync(q.Id,false,ct);
        if(x is null||x.ProfessionalProfileId!=profile.Id)
            return Result.Failure<ProfessionalMissionResponse>(ProfessionalMissionErrors.NotFound);

        return Result.Success(new ProfessionalMissionResponse(
            x.Id.Value,x.EngagementId.Value,x.OrganizationId.Value,x.ProfessionalProfileId.Value,x.BranchId?.Value,
            x.Title,x.Description,x.StartsOn,x.EndsOn,x.TeachingCategoryCodes,x.EstimatedMinutes,x.VehicleProvisionMode.ToString(),
            x.TimeWindows.Select(w=>new MissionTimeWindowInput(w.DayOfWeek,w.StartTime,w.EndTime,w.TimeZoneId)).ToArray(),
            x.Status.ToString(),x.ProposedAtUtc,x.RespondedAtUtc,x.ActivatedAtUtc,x.CompletedAtUtc,x.CancelledAtUtc,x.StatusReason));
    }
}

public sealed class GetProfessionalMissionQueryHandler(
    IProfessionalMissionRepository repo) : IQueryHandler<GetProfessionalMissionQuery,ProfessionalMissionResponse>
{
    public async Task<Result<ProfessionalMissionResponse>> Handle(GetProfessionalMissionQuery q,CancellationToken ct)
    {
        ProfessionalMission? x=await repo.GetAsync(q.Id,false,ct);
        if(x is null)
            return Result.Failure<ProfessionalMissionResponse>(ProfessionalMissionErrors.NotFound);

        if(q.OrganizationId is OrganizationId organizationId&&x.OrganizationId!=organizationId)
            return Result.Failure<ProfessionalMissionResponse>(ProfessionalMissionErrors.NotFound);

        if(q.ProfileId is ProfessionalProfileId profileId&&x.ProfessionalProfileId!=profileId)
            return Result.Failure<ProfessionalMissionResponse>(ProfessionalMissionErrors.NotFound);

        return Result.Success(new ProfessionalMissionResponse(
            x.Id.Value,
            x.EngagementId.Value,
            x.OrganizationId.Value,
            x.ProfessionalProfileId.Value,
            x.BranchId?.Value,
            x.Title,
            x.Description,
            x.StartsOn,
            x.EndsOn,
            x.TeachingCategoryCodes,
            x.EstimatedMinutes,
            x.VehicleProvisionMode.ToString(),
            x.TimeWindows.Select(w=>new MissionTimeWindowInput(w.DayOfWeek,w.StartTime,w.EndTime,w.TimeZoneId)).ToArray(),
            x.Status.ToString(),
            x.ProposedAtUtc,
            x.RespondedAtUtc,
            x.ActivatedAtUtc,
            x.CompletedAtUtc,
            x.CancelledAtUtc,
            x.StatusReason));
    }
}
