using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.EquipmentAssignments;

public enum EquipmentResourceType { Vehicle=1, MobilePhone=2, Tablet=3, Computer=4, Badge=5, Keys=6, TrainingEquipment=7, Other=99 }
public enum EquipmentAssignmentStatus { Planned=1, Active=2, Returned=3, Cancelled=4 }
public enum EquipmentCondition { Unknown=0, New=1, Good=2, Fair=3, Damaged=4, Unusable=5 }

/// <summary>
/// Auditable HR record linking an employee to a resource owned by another bounded context.
/// Workforce owns only assignment dates, handover/return observations and history; it never owns the resource master data.
/// Returned and cancelled assignments are immutable historical records.
/// </summary>
public sealed class EquipmentAssignment : AggregateRoot<EquipmentAssignmentId>, IAuditableEntity
{
    private EquipmentAssignment(){}
    private EquipmentAssignment(EquipmentAssignmentId id,OrganizationId org,EmployeeId employeeId,EquipmentResourceType type,Guid resourceId,DateOnly start,DateOnly? plannedEnd):base(id)
    { OrganizationId=org;EmployeeId=employeeId;ResourceType=type;ResourceId=resourceId;StartDate=start;PlannedEndDate=plannedEnd;Status=EquipmentAssignmentStatus.Planned; }
    public OrganizationId OrganizationId{get;private set;}
    public EmployeeId EmployeeId{get;private set;}
    public EquipmentResourceType ResourceType{get;private set;}
    public Guid ResourceId{get;private set;}
    public DateOnly StartDate{get;private set;}
    public DateOnly? PlannedEndDate{get;private set;}
    public DateOnly? ReturnedOn{get;private set;}
    public EquipmentAssignmentStatus Status{get;private set;}
    public EquipmentCondition HandoverCondition{get;private set;}
    public string? HandoverNotes{get;private set;}
    public DateTimeOffset? HandedOverAtUtc{get;private set;}
    public UserId? HandedOverByUserId{get;private set;}
    public EquipmentCondition ReturnCondition{get;private set;}
    public string? ReturnNotes{get;private set;}
    public DateTimeOffset? ReturnedAtUtc{get;private set;}
    public UserId? ReturnedByUserId{get;private set;}
    public string? CancellationReason{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<EquipmentAssignment> Create(EquipmentAssignmentId id,OrganizationId org,EmployeeId employeeId,EquipmentResourceType type,Guid resourceId,DateOnly start,DateOnly? plannedEnd,DateTimeOffset now,UserId actor)
    { if(id.IsEmpty||org.IsEmpty||employeeId.IsEmpty||resourceId==Guid.Empty)return Result.Failure<EquipmentAssignment>(EquipmentAssignmentErrors.InvalidIdentifier);if(plannedEnd is DateOnly end&&end<start)return Result.Failure<EquipmentAssignment>(EquipmentAssignmentErrors.InvalidPeriod);var x=new EquipmentAssignment(id,org,employeeId,type,resourceId,start,plannedEnd);x.SetCreatedAudit(now,actor);return Result.Success(x); }
    public Result UpdatePlan(DateOnly start,DateOnly? plannedEnd,DateTimeOffset now,UserId actor)
    { if(Status!=EquipmentAssignmentStatus.Planned)return Result.Failure(EquipmentAssignmentErrors.NotEditable);if(plannedEnd is DateOnly end&&end<start)return Result.Failure(EquipmentAssignmentErrors.InvalidPeriod);StartDate=start;PlannedEndDate=plannedEnd;SetModifiedAudit(now,actor);return Result.Success(); }
    public Result HandOver(EquipmentCondition condition,string? notes,DateTimeOffset now,UserId actor)
    { if(Status!=EquipmentAssignmentStatus.Planned)return Result.Failure(EquipmentAssignmentErrors.InvalidTransition);if((notes?.Trim().Length??0)>1000)return Result.Failure(EquipmentAssignmentErrors.InvalidNotes);Status=EquipmentAssignmentStatus.Active;HandoverCondition=condition;HandoverNotes=Normalize(notes);HandedOverAtUtc=now.ToUniversalTime();HandedOverByUserId=actor;SetModifiedAudit(now,actor);return Result.Success(); }
    public Result Return(DateOnly returnedOn,EquipmentCondition condition,string? notes,DateTimeOffset now,UserId actor)
    { if(Status!=EquipmentAssignmentStatus.Active)return Result.Failure(EquipmentAssignmentErrors.InvalidTransition);if(returnedOn<StartDate)return Result.Failure(EquipmentAssignmentErrors.InvalidReturnDate);if((notes?.Trim().Length??0)>1000)return Result.Failure(EquipmentAssignmentErrors.InvalidNotes);Status=EquipmentAssignmentStatus.Returned;ReturnedOn=returnedOn;ReturnCondition=condition;ReturnNotes=Normalize(notes);ReturnedAtUtc=now.ToUniversalTime();ReturnedByUserId=actor;SetModifiedAudit(now,actor);return Result.Success(); }
    public Result Cancel(string reason,DateTimeOffset now,UserId actor)
    { if(Status!=EquipmentAssignmentStatus.Planned)return Result.Failure(EquipmentAssignmentErrors.InvalidTransition);if(string.IsNullOrWhiteSpace(reason))return Result.Failure(EquipmentAssignmentErrors.CancellationReasonRequired);if(reason.Trim().Length>512)return Result.Failure(EquipmentAssignmentErrors.InvalidNotes);Status=EquipmentAssignmentStatus.Cancelled;CancellationReason=reason.Trim();SetModifiedAudit(now,actor);return Result.Success(); }
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string? Normalize(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}
public static class EquipmentAssignmentErrors
{
    public static readonly Error InvalidIdentifier=Error.Validation("Workforce.EquipmentAssignment.InvalidIdentifier","errors.workforce.equipmentAssignment.invalidIdentifier");
    public static readonly Error InvalidPeriod=Error.Validation("Workforce.EquipmentAssignment.InvalidPeriod","errors.workforce.equipmentAssignment.invalidPeriod");
    public static readonly Error InvalidReturnDate=Error.Validation("Workforce.EquipmentAssignment.InvalidReturnDate","errors.workforce.equipmentAssignment.invalidReturnDate");
    public static readonly Error InvalidNotes=Error.Validation("Workforce.EquipmentAssignment.InvalidNotes","errors.workforce.equipmentAssignment.invalidNotes");
    public static readonly Error InvalidTransition=Error.Conflict("Workforce.EquipmentAssignment.InvalidTransition","errors.workforce.equipmentAssignment.invalidTransition");
    public static readonly Error NotEditable=Error.Conflict("Workforce.EquipmentAssignment.NotEditable","errors.workforce.equipmentAssignment.notEditable");
    public static readonly Error ResourceAlreadyAssigned=Error.Conflict("Workforce.EquipmentAssignment.ResourceAlreadyAssigned","errors.workforce.equipmentAssignment.resourceAlreadyAssigned");
    public static readonly Error CancellationReasonRequired=Error.Validation("Workforce.EquipmentAssignment.CancellationReasonRequired","errors.workforce.equipmentAssignment.cancellationReasonRequired");
    public static readonly Error NotFound=Error.NotFound("Workforce.EquipmentAssignment.NotFound","errors.workforce.equipmentAssignment.notFound");
}
public interface IEquipmentAssignmentRepository
{
    Task<EquipmentAssignment?> GetAsync(OrganizationId organizationId,EquipmentAssignmentId id,bool tracking,CancellationToken ct=default);
    Task<IReadOnlyList<EquipmentAssignment>> ListAsync(OrganizationId organizationId,EmployeeId? employeeId,EquipmentAssignmentStatus? status,EquipmentResourceType? type,CancellationToken ct=default);
    Task<bool> HasResourceOverlapAsync(OrganizationId organizationId,EquipmentResourceType type,Guid resourceId,DateOnly from,DateOnly? to,EquipmentAssignmentId? excluding,CancellationToken ct=default);
    void Add(EquipmentAssignment assignment);
}
