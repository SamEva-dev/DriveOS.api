using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.StudentAssignments;

/// <summary>
/// Temporary assignment of one student to an external professional mission.
/// It never transfers ownership of the student dossier to BC-13.
/// </summary>
public sealed class ProfessionalStudentAssignment
    : AggregateRoot<ProfessionalStudentAssignmentId>, IAuditableEntity
{
    private ProfessionalStudentAssignment(){}

    private ProfessionalStudentAssignment(
        ProfessionalStudentAssignmentId id,
        ProfessionalMissionId missionId,
        ProfessionalEngagementId engagementId,
        ProfessionalProfileId professionalProfileId,
        OrganizationId organizationId,
        PersonId studentId,
        DateOnly startsOn,
        DateOnly endsOn,
        string scopeCode,
        UserId responsibleUserId,
        string assignmentReason):base(id)
    {
        MissionId=missionId;
        EngagementId=engagementId;
        ProfessionalProfileId=professionalProfileId;
        OrganizationId=organizationId;
        StudentId=studentId;
        StartsOn=startsOn;
        EndsOn=endsOn;
        ScopeCode=NormalizeScope(scopeCode);
        ResponsibleUserId=responsibleUserId;
        AssignmentReason=NormalizeReason(assignmentReason);
        Status=ProfessionalStudentAssignmentStatus.Active;
    }

    public ProfessionalMissionId MissionId{get;private set;}
    public ProfessionalEngagementId EngagementId{get;private set;}
    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public OrganizationId OrganizationId{get;private set;}
    public PersonId StudentId{get;private set;}
    public DateOnly StartsOn{get;private set;}
    public DateOnly EndsOn{get;private set;}
    public string ScopeCode{get;private set;}=string.Empty;
    public UserId ResponsibleUserId{get;private set;}
    public string AssignmentReason{get;private set;}=string.Empty;
    public ProfessionalStudentAssignmentStatus Status{get;private set;}
    public DateTimeOffset? RevokedAtUtc{get;private set;}
    public UserId? RevokedByUserId{get;private set;}
    public string? RevocationReason{get;private set;}

    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<ProfessionalStudentAssignment> Create(
        ProfessionalStudentAssignmentId id,
        ProfessionalMissionId missionId,
        ProfessionalEngagementId engagementId,
        ProfessionalProfileId professionalProfileId,
        OrganizationId organizationId,
        PersonId studentId,
        DateOnly startsOn,
        DateOnly endsOn,
        string scopeCode,
        UserId responsibleUserId,
        string assignmentReason,
        DateOnly missionStartsOn,
        DateOnly missionEndsOn,
        DateTimeOffset now,
        UserId actor)
    {
        if(id.IsEmpty||missionId.IsEmpty||engagementId.IsEmpty||professionalProfileId.IsEmpty||
           organizationId.IsEmpty||studentId.IsEmpty||responsibleUserId.IsEmpty)
            return Result.Failure<ProfessionalStudentAssignment>(
                ProfessionalStudentAssignmentErrors.InvalidIdentifier);

        if(endsOn<startsOn||startsOn<missionStartsOn||endsOn>missionEndsOn)
            return Result.Failure<ProfessionalStudentAssignment>(
                ProfessionalStudentAssignmentErrors.OutsideMissionPeriod);

        string scope=NormalizeScope(scopeCode);
        if(scope.Length is <2 or >80)
            return Result.Failure<ProfessionalStudentAssignment>(
                ProfessionalStudentAssignmentErrors.InvalidScope);

        string reason=NormalizeReason(assignmentReason);
        if(reason.Length is <2 or >512)
            return Result.Failure<ProfessionalStudentAssignment>(
                ProfessionalStudentAssignmentErrors.AssignmentReasonRequired);

        var assignment=new ProfessionalStudentAssignment(
            id,missionId,engagementId,professionalProfileId,organizationId,studentId,
            startsOn,endsOn,scope,responsibleUserId,reason);

        assignment.SetCreatedAudit(now,actor);
        return Result.Success(assignment);
    }

    public Result Revoke(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalStudentAssignmentStatus.Active)
            return Result.Failure(ProfessionalStudentAssignmentErrors.InvalidTransition);

        reason=(reason??string.Empty).Trim();
        if(reason.Length is <2 or >512)
            return Result.Failure(ProfessionalStudentAssignmentErrors.RevocationReasonRequired);

        Status=ProfessionalStudentAssignmentStatus.Revoked;
        RevokedAtUtc=now.ToUniversalTime();
        RevokedByUserId=actor;
        RevocationReason=reason;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string NormalizeScope(string? value)=>(value??string.Empty).Trim().ToUpperInvariant();
    private static string NormalizeReason(string? value)=>(value??string.Empty).Trim();
}

public enum ProfessionalStudentAssignmentStatus
{
    Active=1,
    Revoked=2,
    Completed=3
}
