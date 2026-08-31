using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.StudentAssignments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.StudentAssignments;

public sealed class AssignStudentToProfessionalMissionCommandHandler(
    IProfessionalStudentAssignmentRepository assignments,
    IProfessionalMissionRepository missions,
    IExternalAccessGrantRepository grants,
    IProfessionalStudentScopeGateway students,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<AssignStudentToProfessionalMissionCommand,ProfessionalStudentAssignmentId>
{
    public async Task<Result<ProfessionalStudentAssignmentId>> Handle(
        AssignStudentToProfessionalMissionCommand c,CancellationToken ct)
    {
        ProfessionalMission? mission=await missions.GetAsync(c.MissionId,false,ct);
        if(mission is null||mission.OrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalStudentAssignmentId>(ProfessionalMissionErrors.NotFound);

        if(mission.Status!=ProfessionalMissionStatus.Active)
            return Result.Failure<ProfessionalStudentAssignmentId>(
                ProfessionalStudentAssignmentErrors.ActiveMissionRequired);

        if(!await students.ExistsAsync(c.OrganizationId,c.StudentId,ct))
            return Result.Failure<ProfessionalStudentAssignmentId>(
                ProfessionalStudentAssignmentErrors.StudentNotFound);

        if(await assignments.ExistsActiveAsync(mission.Id,c.StudentId,ct))
            return Result.Failure<ProfessionalStudentAssignmentId>(
                ProfessionalStudentAssignmentErrors.DuplicateAssignment);

        var created=ProfessionalStudentAssignment.Create(
            c.Id,mission.Id,mission.EngagementId,mission.ProfessionalProfileId,mission.OrganizationId,
            c.StudentId,c.StartsOn,c.EndsOn,c.ScopeCode,c.ActorUserId,c.AssignmentReason,
            mission.StartsOn,mission.EndsOn,clock.UtcNow,c.ActorUserId);

        if(created.IsFailure)
            return Result.Failure<ProfessionalStudentAssignmentId>(created.Error);

        assignments.Add(created.Value);

        // Access grant is created atomically with the assignment.
        var grant=ExternalAccessGrant.Create(
            new ExternalAccessGrantId(Guid.NewGuid()),
            mission.EngagementId,
            mission.ProfessionalProfileId,
            mission.OrganizationId,
            mission.BranchId,
            ExternalAccessResourceType.Student,
            c.StudentId.Value,
            "READ",
            c.StartsOn,
            c.EndsOn,
            mission.StartsOn,
            mission.EndsOn,
            clock.UtcNow,
            c.ActorUserId);

        if(grant.IsFailure)
            return Result.Failure<ProfessionalStudentAssignmentId>(grant.Error);

        grants.Add(grant.Value);
        await uow.CommitAsync(ct);
        return Result.Success(created.Value.Id);
    }
}

public sealed class RevokeProfessionalStudentAssignmentCommandHandler(
    IProfessionalStudentAssignmentRepository assignments,
    IExternalAccessGrantRepository grants,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<RevokeProfessionalStudentAssignmentCommand>
{
    public async Task<Result> Handle(RevokeProfessionalStudentAssignmentCommand c,CancellationToken ct)
    {
        ProfessionalStudentAssignment? assignment=await assignments.GetAsync(c.Id,true,ct);
        if(assignment is null||assignment.OrganizationId!=c.OrganizationId)
            return Result.Failure(ProfessionalStudentAssignmentErrors.NotFound);

        Result revoked=assignment.Revoke(c.Reason,clock.UtcNow,c.ActorUserId);
        if(revoked.IsFailure)return revoked;

        var activeGrants=await grants.ListByEngagementAsync(assignment.EngagementId,ct);
        foreach(var grantSnapshot in activeGrants.Where(x=>
            x.ResourceType==ExternalAccessResourceType.Student&&
            x.ResourceId==assignment.StudentId.Value&&
            x.Status==ExternalAccessGrantStatus.Active))
        {
            var tracked=await grants.GetAsync(grantSnapshot.Id,true,ct);
            if(tracked is not null)
            {
                Result accessRevoked=tracked.Revoke(c.Reason,clock.UtcNow,c.ActorUserId);
                if(accessRevoked.IsFailure)return accessRevoked;
            }
        }

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class GetProfessionalMissionStudentAssignmentsQueryHandler(
    IProfessionalStudentAssignmentRepository assignments,
    IProfessionalMissionRepository missions,
    IProfessionalStudentScopeGateway students)
    :IQueryHandler<GetProfessionalMissionStudentAssignmentsQuery,IReadOnlyList<ProfessionalStudentAssignmentListItem>>
{
    public async Task<Result<IReadOnlyList<ProfessionalStudentAssignmentListItem>>> Handle(
        GetProfessionalMissionStudentAssignmentsQuery query,CancellationToken ct)
    {
        ProfessionalMission? mission=await missions.GetAsync(query.MissionId,false,ct);
        if(mission is null||mission.OrganizationId!=query.OrganizationId)
            return Result.Failure<IReadOnlyList<ProfessionalStudentAssignmentListItem>>(ProfessionalMissionErrors.NotFound);

        IReadOnlyList<ProfessionalStudentAssignment> source=await assignments.ListByMissionAsync(query.MissionId,ct);
        var result=new List<ProfessionalStudentAssignmentListItem>(source.Count);
        foreach(ProfessionalStudentAssignment assignment in source)
        {
            ProfessionalStudentScopeStudent? student=await students.GetAsync(query.OrganizationId,assignment.StudentId,ct);
            result.Add(new ProfessionalStudentAssignmentListItem(
                assignment.Id.Value,
                assignment.MissionId.Value,
                assignment.EngagementId.Value,
                assignment.ProfessionalProfileId.Value,
                assignment.StudentId.Value,
                student?.DisplayName??assignment.StudentId.Value.ToString(),
                student?.Email,
                student?.Phone,
                assignment.StartsOn,
                assignment.EndsOn,
                assignment.ScopeCode,
                assignment.AssignmentReason,
                assignment.Status.ToString(),
                assignment.CreatedAtUtc,
                assignment.RevokedAtUtc,
                assignment.RevocationReason));
        }
        return Result.Success<IReadOnlyList<ProfessionalStudentAssignmentListItem>>(result);
    }
}


public sealed class GetCurrentProfessionalStudentAssignmentsQueryHandler(
    IProfessionalProfileRepository profiles,
    IProfessionalStudentAssignmentRepository assignments,
    IProfessionalMissionRepository missions,
    IExternalAccessGrantRepository grants,
    IProfessionalStudentScopeGateway students,
    IClock clock)
    :IQueryHandler<GetCurrentProfessionalStudentAssignmentsQuery,IReadOnlyList<ProfessionalStudentAssignmentListItem>>
{
    public async Task<Result<IReadOnlyList<ProfessionalStudentAssignmentListItem>>> Handle(
        GetCurrentProfessionalStudentAssignmentsQuery query,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(query.UserId,ct);
        if(profile is null)
            return Result.Failure<IReadOnlyList<ProfessionalStudentAssignmentListItem>>(ProfessionalProfileErrors.NotFound);

        DateOnly today=DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        IReadOnlyList<ProfessionalStudentAssignment> source=await assignments.ListActiveByProfileAsync(profile.Id,ct);
        var result=new List<ProfessionalStudentAssignmentListItem>();
        foreach(ProfessionalStudentAssignment assignment in source)
        {
            if(today<assignment.StartsOn||today>assignment.EndsOn)continue;
            ProfessionalMission? mission=await missions.GetAsync(assignment.MissionId,false,ct);
            if(mission is null||mission.ProfessionalProfileId!=profile.Id||mission.Status!=ProfessionalMissionStatus.Active)continue;
            bool allowed=await grants.HasEffectiveGrantAsync(
                profile.Id,assignment.OrganizationId,ExternalAccessResourceType.Student,assignment.StudentId.Value,"READ",today,ct);
            if(!allowed)continue;

            ProfessionalStudentScopeStudent? student=await students.GetAsync(assignment.OrganizationId,assignment.StudentId,ct);
            result.Add(Map(assignment,student));
        }
        return Result.Success<IReadOnlyList<ProfessionalStudentAssignmentListItem>>(result);
    }

    internal static ProfessionalStudentAssignmentListItem Map(
        ProfessionalStudentAssignment assignment,ProfessionalStudentScopeStudent? student)=>new(
        assignment.Id.Value,assignment.MissionId.Value,assignment.EngagementId.Value,assignment.ProfessionalProfileId.Value,
        assignment.StudentId.Value,student?.DisplayName??assignment.StudentId.Value.ToString(),student?.Email,student?.Phone,
        assignment.StartsOn,assignment.EndsOn,assignment.ScopeCode,assignment.AssignmentReason,assignment.Status.ToString(),
        assignment.CreatedAtUtc,assignment.RevokedAtUtc,assignment.RevocationReason);
}

public sealed class GetCurrentProfessionalMissionStudentAssignmentsQueryHandler(
    IProfessionalProfileRepository profiles,
    IProfessionalMissionRepository missions,
    IProfessionalStudentAssignmentRepository assignments,
    IExternalAccessGrantRepository grants,
    IProfessionalStudentScopeGateway students,
    IClock clock)
    :IQueryHandler<GetCurrentProfessionalMissionStudentAssignmentsQuery,IReadOnlyList<ProfessionalStudentAssignmentListItem>>
{
    public async Task<Result<IReadOnlyList<ProfessionalStudentAssignmentListItem>>> Handle(
        GetCurrentProfessionalMissionStudentAssignmentsQuery query,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(query.UserId,ct);
        if(profile is null)
            return Result.Failure<IReadOnlyList<ProfessionalStudentAssignmentListItem>>(ProfessionalProfileErrors.NotFound);

        ProfessionalMission? mission=await missions.GetAsync(query.MissionId,false,ct);
        if(mission is null||mission.ProfessionalProfileId!=profile.Id||mission.Status!=ProfessionalMissionStatus.Active)
            return Result.Failure<IReadOnlyList<ProfessionalStudentAssignmentListItem>>(ProfessionalMissionErrors.NotFound);

        DateOnly today=DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        IReadOnlyList<ProfessionalStudentAssignment> source=await assignments.ListByMissionAsync(query.MissionId,ct);
        var result=new List<ProfessionalStudentAssignmentListItem>();
        foreach(ProfessionalStudentAssignment assignment in source.Where(x=>x.Status==ProfessionalStudentAssignmentStatus.Active))
        {
            if(today<assignment.StartsOn||today>assignment.EndsOn)continue;
            bool allowed=await grants.HasEffectiveGrantAsync(
                profile.Id,assignment.OrganizationId,ExternalAccessResourceType.Student,assignment.StudentId.Value,"READ",today,ct);
            if(!allowed)continue;
            ProfessionalStudentScopeStudent? student=await students.GetAsync(assignment.OrganizationId,assignment.StudentId,ct);
            result.Add(GetCurrentProfessionalStudentAssignmentsQueryHandler.Map(assignment,student));
        }
        return Result.Success<IReadOnlyList<ProfessionalStudentAssignmentListItem>>(result);
    }
}
