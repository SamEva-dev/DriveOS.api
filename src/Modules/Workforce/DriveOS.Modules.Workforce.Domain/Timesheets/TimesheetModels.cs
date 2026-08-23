using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.Timesheets;

public enum TimesheetStatus { Draft=1, Submitted=2, UnderReview=3, Approved=4, Rejected=5, Locked=6 }
public enum TimesheetActivityType { Teaching=1, Exam=2, Administrative=3, Travel=4, Meeting=5, Training=6, Leave=7, Other=99 }
public enum TimesheetEntrySource { Manual=1, Scheduling=2, TrainingDelivery=3, Leave=4 }

/// <summary>
/// Durable HR record of one employee's declared working time for a bounded period.
/// Source systems may suggest lines, but once persisted this aggregate owns review, approval and locking.
/// Locked timesheets are immutable audit records and are never rewritten from Scheduling or Training Delivery.
/// </summary>
public sealed class Timesheet : AggregateRoot<TimesheetId>, IAuditableEntity
{
    private readonly List<TimesheetEntry> _entries=[];
    private Timesheet(){}
    private Timesheet(TimesheetId id,OrganizationId org,EmployeeId employeeId,DateOnly from,DateOnly to):base(id){OrganizationId=org;EmployeeId=employeeId;PeriodFrom=from;PeriodTo=to;Status=TimesheetStatus.Draft;}
    public OrganizationId OrganizationId{get;private set;}
    public EmployeeId EmployeeId{get;private set;}
    public DateOnly PeriodFrom{get;private set;}
    public DateOnly PeriodTo{get;private set;}
    public TimesheetStatus Status{get;private set;}
    public DateTimeOffset? SubmittedAtUtc{get;private set;}
    public UserId? SubmittedByUserId{get;private set;}
    public DateTimeOffset? ReviewStartedAtUtc{get;private set;}
    public UserId? ReviewerUserId{get;private set;}
    public DateTimeOffset? DecidedAtUtc{get;private set;}
    public UserId? DecidedByUserId{get;private set;}
    public string? DecisionReason{get;private set;}
    public DateTimeOffset? LockedAtUtc{get;private set;}
    public UserId? LockedByUserId{get;private set;}
    public IReadOnlyCollection<TimesheetEntry> Entries=>_entries;
    public decimal TotalHours=>_entries.Sum(x=>x.Hours);
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}
    public static Result<Timesheet> Create(TimesheetId id,OrganizationId org,EmployeeId employeeId,DateOnly from,DateOnly to,DateTimeOffset now,UserId actor){if(id.IsEmpty||org.IsEmpty||employeeId.IsEmpty)return Result.Failure<Timesheet>(TimesheetErrors.InvalidIdentifier);if(to<from)return Result.Failure<Timesheet>(TimesheetErrors.InvalidPeriod);var x=new Timesheet(id,org,employeeId,from,to);x.SetCreatedAudit(now,actor);return Result.Success(x);}
    public Result<TimesheetEntryId> AddEntry(TimesheetEntryId id,DateOnly date,TimesheetActivityType activityType,decimal hours,string? description,TimesheetEntrySource source,string? sourceReference,DateTimeOffset now,UserId actor){if(Status is not (TimesheetStatus.Draft or TimesheetStatus.Rejected))return Result.Failure<TimesheetEntryId>(TimesheetErrors.NotEditable);if(date<PeriodFrom||date>PeriodTo)return Result.Failure<TimesheetEntryId>(TimesheetErrors.EntryOutsidePeriod);var created=TimesheetEntry.Create(id,date,activityType,hours,description,source,sourceReference);if(created.IsFailure)return Result.Failure<TimesheetEntryId>(created.Error);_entries.Add(created.Value);SetModifiedAudit(now,actor);return Result.Success(id);}
    public Result UpdateEntry(TimesheetEntryId id,DateOnly date,TimesheetActivityType activityType,decimal hours,string? description,DateTimeOffset now,UserId actor){if(Status is not (TimesheetStatus.Draft or TimesheetStatus.Rejected))return Result.Failure(TimesheetErrors.NotEditable);var e=_entries.SingleOrDefault(x=>x.Id==id);if(e is null)return Result.Failure(TimesheetErrors.EntryNotFound);if(date<PeriodFrom||date>PeriodTo)return Result.Failure(TimesheetErrors.EntryOutsidePeriod);var r=e.Update(date,activityType,hours,description);if(r.IsFailure)return r;SetModifiedAudit(now,actor);return Result.Success();}
    public Result RemoveEntry(TimesheetEntryId id,DateTimeOffset now,UserId actor){if(Status is not (TimesheetStatus.Draft or TimesheetStatus.Rejected))return Result.Failure(TimesheetErrors.NotEditable);var e=_entries.SingleOrDefault(x=>x.Id==id);if(e is null)return Result.Failure(TimesheetErrors.EntryNotFound);_entries.Remove(e);SetModifiedAudit(now,actor);return Result.Success();}
    public Result Submit(DateTimeOffset now,UserId actor){if(Status is not (TimesheetStatus.Draft or TimesheetStatus.Rejected))return Result.Failure(TimesheetErrors.InvalidTransition);if(_entries.Count==0)return Result.Failure(TimesheetErrors.EmptyTimesheet);Status=TimesheetStatus.Submitted;SubmittedAtUtc=now.ToUniversalTime();SubmittedByUserId=actor;DecisionReason=null;SetModifiedAudit(now,actor);return Result.Success();}
    public Result StartReview(DateTimeOffset now,UserId actor){if(Status!=TimesheetStatus.Submitted)return Result.Failure(TimesheetErrors.InvalidTransition);Status=TimesheetStatus.UnderReview;ReviewStartedAtUtc=now.ToUniversalTime();ReviewerUserId=actor;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Approve(DateTimeOffset now,UserId actor,string? reason){if(Status is not (TimesheetStatus.Submitted or TimesheetStatus.UnderReview))return Result.Failure(TimesheetErrors.InvalidTransition);Status=TimesheetStatus.Approved;DecidedAtUtc=now.ToUniversalTime();DecidedByUserId=actor;DecisionReason=string.IsNullOrWhiteSpace(reason)?null:reason.Trim();SetModifiedAudit(now,actor);return Result.Success();}
    public Result Reject(DateTimeOffset now,UserId actor,string reason){if(Status is not (TimesheetStatus.Submitted or TimesheetStatus.UnderReview))return Result.Failure(TimesheetErrors.InvalidTransition);if(string.IsNullOrWhiteSpace(reason))return Result.Failure(TimesheetErrors.DecisionReasonRequired);Status=TimesheetStatus.Rejected;DecidedAtUtc=now.ToUniversalTime();DecidedByUserId=actor;DecisionReason=reason.Trim();SetModifiedAudit(now,actor);return Result.Success();}
    public Result Lock(DateTimeOffset now,UserId actor){if(Status!=TimesheetStatus.Approved)return Result.Failure(TimesheetErrors.InvalidTransition);Status=TimesheetStatus.Locked;LockedAtUtc=now.ToUniversalTime();LockedByUserId=actor;SetModifiedAudit(now,actor);return Result.Success();}
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
}

public sealed class TimesheetEntry
{
    private TimesheetEntry(){}
    private TimesheetEntry(TimesheetEntryId id,DateOnly date,TimesheetActivityType type,decimal hours,string? description,TimesheetEntrySource source,string? sourceReference){Id=id;Date=date;ActivityType=type;Hours=hours;Description=Normalize(description,512);Source=source;SourceReference=Normalize(sourceReference,128);}
    public TimesheetEntryId Id{get;private set;}
    public DateOnly Date{get;private set;}
    public TimesheetActivityType ActivityType{get;private set;}
    public decimal Hours{get;private set;}
    public string? Description{get;private set;}
    public TimesheetEntrySource Source{get;private set;}
    public string? SourceReference{get;private set;}
    public static Result<TimesheetEntry> Create(TimesheetEntryId id,DateOnly date,TimesheetActivityType type,decimal hours,string? description,TimesheetEntrySource source,string? sourceReference){if(id.IsEmpty)return Result.Failure<TimesheetEntry>(TimesheetErrors.InvalidIdentifier);if(hours<=0||hours>24)return Result.Failure<TimesheetEntry>(TimesheetErrors.InvalidHours);if((description?.Trim().Length??0)>512||(sourceReference?.Trim().Length??0)>128)return Result.Failure<TimesheetEntry>(TimesheetErrors.InvalidText);return Result.Success(new TimesheetEntry(id,date,type,hours,description,source,sourceReference));}
    internal Result Update(DateOnly date,TimesheetActivityType type,decimal hours,string? description){if(hours<=0||hours>24)return Result.Failure(TimesheetErrors.InvalidHours);if((description?.Trim().Length??0)>512)return Result.Failure(TimesheetErrors.InvalidText);Date=date;ActivityType=type;Hours=hours;Description=Normalize(description,512);return Result.Success();}
    private static string? Normalize(string? v,int max)=>string.IsNullOrWhiteSpace(v)?null:v.Trim()[..Math.Min(v.Trim().Length,max)];
}

public static class TimesheetErrors
{
    public static readonly Error InvalidIdentifier=Error.Validation("Workforce.Timesheet.InvalidIdentifier","errors.workforce.timesheet.invalidIdentifier");
    public static readonly Error InvalidPeriod=Error.Validation("Workforce.Timesheet.InvalidPeriod","errors.workforce.timesheet.invalidPeriod");
    public static readonly Error OverlappingPeriod=Error.Conflict("Workforce.Timesheet.OverlappingPeriod","errors.workforce.timesheet.overlappingPeriod");
    public static readonly Error EntryOutsidePeriod=Error.Validation("Workforce.Timesheet.EntryOutsidePeriod","errors.workforce.timesheet.entryOutsidePeriod");
    public static readonly Error InvalidHours=Error.Validation("Workforce.Timesheet.InvalidHours","errors.workforce.timesheet.invalidHours");
    public static readonly Error InvalidText=Error.Validation("Workforce.Timesheet.InvalidText","errors.workforce.timesheet.invalidText");
    public static readonly Error EmptyTimesheet=Error.Validation("Workforce.Timesheet.Empty","errors.workforce.timesheet.empty");
    public static readonly Error NotEditable=Error.Conflict("Workforce.Timesheet.NotEditable","errors.workforce.timesheet.notEditable");
    public static readonly Error InvalidTransition=Error.Conflict("Workforce.Timesheet.InvalidTransition","errors.workforce.timesheet.invalidTransition");
    public static readonly Error DecisionReasonRequired=Error.Validation("Workforce.Timesheet.DecisionReasonRequired","errors.workforce.timesheet.decisionReasonRequired");
    public static readonly Error EntryNotFound=Error.NotFound("Workforce.Timesheet.EntryNotFound","errors.workforce.timesheet.entryNotFound");
    public static readonly Error NotFound=Error.NotFound("Workforce.Timesheet.NotFound","errors.workforce.timesheet.notFound");
}
public interface ITimesheetRepository
{
    Task<Timesheet?> GetAsync(OrganizationId organizationId,TimesheetId id,bool tracking,CancellationToken ct=default);
    Task<IReadOnlyList<Timesheet>> ListAsync(OrganizationId organizationId,EmployeeId? employeeId,TimesheetStatus? status,DateOnly? from,DateOnly? to,CancellationToken ct=default);
    Task<bool> HasOverlapAsync(OrganizationId organizationId,EmployeeId employeeId,DateOnly from,DateOnly to,TimesheetId? excluding,CancellationToken ct=default);
    void Add(Timesheet timesheet);
}
