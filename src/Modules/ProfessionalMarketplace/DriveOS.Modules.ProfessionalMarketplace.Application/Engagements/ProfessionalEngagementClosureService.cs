using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Notifications;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.StudentAssignments;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;

public sealed class ProfessionalEngagementClosureService(
    IProfessionalMissionRepository missions,
    IProfessionalStudentAssignmentRepository assignments,
    IExternalAccessGrantRepository grants,
    IProfessionalProfileRepository profiles,
    IMarketplaceNotificationGateway notifications,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock)
{
    public async Task<Result<ProfessionalEngagementClosureResult>> CloseAsync(
        ProfessionalEngagement engagement,
        ProfessionalEngagementClosureMode mode,
        string reason,
        UserId actor,
        CancellationToken ct)
    {
        DateOnly today=DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        string closureReason=string.IsNullOrWhiteSpace(reason)
            ? mode==ProfessionalEngagementClosureMode.Completed
                ? "Engagement completed"
                : "Engagement terminated"
            : reason.Trim();

        IReadOnlyList<ProfessionalMission> missionSnapshots=
            await missions.ListByEngagementAsync(engagement.Id,ct);

        int completed=0;
        int cancelled=0;

        foreach(ProfessionalMission snapshot in missionSnapshots)
        {
            if(snapshot.Status is ProfessionalMissionStatus.Completed or
               ProfessionalMissionStatus.Cancelled or
               ProfessionalMissionStatus.Declined)
                continue;

            ProfessionalMission? mission=await missions.GetAsync(snapshot.Id,true,ct);
            if(mission is null)continue;

            if(mode==ProfessionalEngagementClosureMode.Completed &&
               (mission.Status is ProfessionalMissionStatus.Active or ProfessionalMissionStatus.Paused) &&
               today>=mission.EndsOn)
            {
                Result done=mission.Complete(today,clock.UtcNow,actor);
                if(done.IsFailure)
                    return Result.Failure<ProfessionalEngagementClosureResult>(done.Error);
                completed++;
            }
            else
            {
                Result cancel=mission.Cancel(closureReason,clock.UtcNow,actor);
                if(cancel.IsFailure)
                    return Result.Failure<ProfessionalEngagementClosureResult>(cancel.Error);
                cancelled++;
            }
        }

        IReadOnlyList<ProfessionalStudentAssignment> assignmentSnapshots=
            await assignments.ListActiveByEngagementAsync(engagement.Id,ct);

        int revokedAssignments=0;
        foreach(ProfessionalStudentAssignment snapshot in assignmentSnapshots)
        {
            ProfessionalStudentAssignment? assignment=
                await assignments.GetAsync(snapshot.Id,true,ct);

            if(assignment is null||assignment.Status!=ProfessionalStudentAssignmentStatus.Active)
                continue;

            Result revoked=assignment.Revoke(closureReason,clock.UtcNow,actor);
            if(revoked.IsFailure)
                return Result.Failure<ProfessionalEngagementClosureResult>(revoked.Error);

            revokedAssignments++;
        }

        IReadOnlyList<ExternalAccessGrant> accessSnapshots=
            await grants.ListByEngagementAsync(engagement.Id,ct);

        int revokedGrants=0;
        foreach(ExternalAccessGrant snapshot in accessSnapshots.Where(x=>x.Status==ExternalAccessGrantStatus.Active))
        {
            ExternalAccessGrant? grant=await grants.GetAsync(snapshot.Id,true,ct);
            if(grant is null||grant.Status!=ExternalAccessGrantStatus.Active)
                continue;

            Result revoked=grant.Revoke(closureReason,clock.UtcNow,actor);
            if(revoked.IsFailure)
                return Result.Failure<ProfessionalEngagementClosureResult>(revoked.Error);

            revokedGrants++;
        }

        Result engagementResult=mode==ProfessionalEngagementClosureMode.Completed
            ? engagement.Complete(today,clock.UtcNow,actor)
            : engagement.Terminate(closureReason,clock.UtcNow,actor);

        if(engagementResult.IsFailure)
            return Result.Failure<ProfessionalEngagementClosureResult>(engagementResult.Error);

        await uow.CommitAsync(ct);

        ProfessionalProfile? profile=await profiles.GetByIdAsync(engagement.ProfessionalProfileId,ct);
        if(profile is not null&&profile.UserId is UserId professionalUserId&&!professionalUserId.IsEmpty)
        {
            await notifications.TryEnqueueAsync(new(
                "User",
                professionalUserId.Value,
                engagement.OrganizationId,
                "ENGAGEMENT",
                mode==ProfessionalEngagementClosureMode.Completed
                    ?"professionalMarketplace.notifications.engagementCompleted"
                    :"professionalMarketplace.notifications.engagementTerminated",
                $"engagement-closed:{engagement.Id.Value}:{engagement.Status}",
                new Dictionary<string,string?>
                {
                    ["engagementId"]=engagement.Id.Value.ToString(),
                    ["status"]=engagement.Status.ToString(),
                    ["reason"]=engagement.StatusReason,
                    ["endedAtUtc"]=engagement.EndedAtUtc?.ToString("O")
                },
                "PROFESSIONAL_ENGAGEMENT",
                engagement.Id.Value,
                profile.ProfessionalEmail,
                profile.Languages.FirstOrDefault(x=>x.StartsWith("fr",StringComparison.OrdinalIgnoreCase))??"en",
                actor),ct);
        }

        return Result.Success(new ProfessionalEngagementClosureResult(
            engagement.Status.ToString(),
            completed,
            cancelled,
            revokedAssignments,
            revokedGrants,
            true));
    }
}

public enum ProfessionalEngagementClosureMode
{
    Completed=1,
    Terminated=2
}
