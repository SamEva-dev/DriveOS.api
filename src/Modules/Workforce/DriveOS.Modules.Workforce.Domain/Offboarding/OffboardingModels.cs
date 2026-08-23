using DriveOS.SharedKernel.Auditing;
using DriveOS.Modules.Workforce.Domain.Events;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.Offboarding;

public enum OffboardingStatus { InProgress=1, ReadyToComplete=2, Completed=3, Cancelled=4 }
public enum OffboardingChecklistItemStatus { Pending=1, Completed=2, Waived=3 }
public enum OffboardingChecklistItemKind
{
    FutureSchedulingReviewed=1,
    BranchAssignmentsClosed=2,
    JobPositionsClosed=3,
    EmploymentContractsClosed=4,
    EquipmentReturned=5,
    TimesheetsFinalized=6,
    ProfessionalRestrictionsReviewed=7,
    AccessRevocationPrepared=8,
    EmployeeDocumentsReviewed=9
}

/// <summary>
/// Durable offboarding workflow for one ending employment relationship. It never deletes the Employee or the global User account.
/// Local dependency items can be re-evaluated from authoritative Workforce data; manual/external items require an explicit actor decision.
/// </summary>
public sealed class OffboardingProcess : AggregateRoot<OffboardingProcessId>, IAuditableEntity
{
    private readonly List<OffboardingChecklistItem> _items=[];
    private OffboardingProcess(){}
    private OffboardingProcess(OffboardingProcessId id,OrganizationId organizationId,EmployeeId employeeId,DateOnly plannedEndDate,string reason):base(id)
    { OrganizationId=organizationId;EmployeeId=employeeId;PlannedEndDate=plannedEndDate;Reason=reason.Trim();Status=OffboardingStatus.InProgress; }
    public OrganizationId OrganizationId{get;private set;}
    public EmployeeId EmployeeId{get;private set;}
    public DateOnly PlannedEndDate{get;private set;}
    public string Reason{get;private set;}=string.Empty;
    public OffboardingStatus Status{get;private set;}
    public DateTimeOffset? CompletedAtUtc{get;private set;}
    public UserId? CompletedByUserId{get;private set;}
    public DateTimeOffset? CancelledAtUtc{get;private set;}
    public UserId? CancelledByUserId{get;private set;}
    public string? CancellationReason{get;private set;}
    public IReadOnlyCollection<OffboardingChecklistItem> Items=>_items;
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<OffboardingProcess> Create(OffboardingProcessId id,OrganizationId organizationId,EmployeeId employeeId,DateOnly plannedEndDate,string reason,DateTimeOffset now,UserId actor,UserId? linkedUserId=null)
    {
        if(id.IsEmpty||organizationId.IsEmpty||employeeId.IsEmpty)return Result.Failure<OffboardingProcess>(OffboardingErrors.InvalidIdentifier);
        if(string.IsNullOrWhiteSpace(reason))return Result.Failure<OffboardingProcess>(OffboardingErrors.ReasonRequired);
        var x=new OffboardingProcess(id,organizationId,employeeId,plannedEndDate,reason);
        foreach(var kind in Enum.GetValues<OffboardingChecklistItemKind>()) x._items.Add(OffboardingChecklistItem.Create(OffboardingChecklistItemId.New(),kind,IsAutomatic(kind)));
        x.SetCreatedAudit(now,actor);
        x.RaiseDomainEvent(new OffboardingStartedDomainEvent(Guid.NewGuid(),now.ToUniversalTime(),id,employeeId,organizationId,plannedEndDate,linkedUserId,actor));
        return Result.Success(x);
    }
    public Result SynchronizeAutomaticItem(OffboardingChecklistItemKind kind,int blockerCount,string? note,DateTimeOffset now,UserId actor)
    {
        if(Status is OffboardingStatus.Completed or OffboardingStatus.Cancelled)return Result.Failure(OffboardingErrors.NotEditable);
        var item=_items.SingleOrDefault(x=>x.Kind==kind); if(item is null)return Result.Failure(OffboardingErrors.ItemNotFound);
        if(!item.IsAutomatic)return Result.Failure(OffboardingErrors.ManualItemExpected);
        item.Synchronize(blockerCount,note,now,actor); RecalculateStatus(); SetModifiedAudit(now,actor); return Result.Success();
    }
    public Result CompleteManualItem(OffboardingChecklistItemKind kind,string? note,DateTimeOffset now,UserId actor)
    {
        if(Status is OffboardingStatus.Completed or OffboardingStatus.Cancelled)return Result.Failure(OffboardingErrors.NotEditable);
        var item=_items.SingleOrDefault(x=>x.Kind==kind); if(item is null)return Result.Failure(OffboardingErrors.ItemNotFound);
        if(item.IsAutomatic)return Result.Failure(OffboardingErrors.AutomaticItemExpected);
        item.Complete(note,now,actor); RecalculateStatus(); SetModifiedAudit(now,actor); return Result.Success();
    }
    public Result WaiveItem(OffboardingChecklistItemKind kind,string reason,DateTimeOffset now,UserId actor)
    {
        if(Status is OffboardingStatus.Completed or OffboardingStatus.Cancelled)return Result.Failure(OffboardingErrors.NotEditable);
        if(string.IsNullOrWhiteSpace(reason))return Result.Failure(OffboardingErrors.WaiverReasonRequired);
        var item=_items.SingleOrDefault(x=>x.Kind==kind); if(item is null)return Result.Failure(OffboardingErrors.ItemNotFound);
        item.Waive(reason,now,actor); RecalculateStatus(); SetModifiedAudit(now,actor); return Result.Success();
    }
    public Result Complete(DateTimeOffset now,UserId actor,UserId? linkedUserId=null)
    {
        if(Status!=OffboardingStatus.ReadyToComplete)return Result.Failure(OffboardingErrors.ChecklistIncomplete);
        Status=OffboardingStatus.Completed;CompletedAtUtc=now.ToUniversalTime();CompletedByUserId=actor;SetModifiedAudit(now,actor);RaiseDomainEvent(new OffboardingCompletedDomainEvent(Guid.NewGuid(),now.ToUniversalTime(),Id,EmployeeId,OrganizationId,PlannedEndDate,linkedUserId,actor));return Result.Success();
    }
    public Result Cancel(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status==OffboardingStatus.Completed)return Result.Failure(OffboardingErrors.InvalidTransition);
        if(Status==OffboardingStatus.Cancelled)return Result.Success();
        if(string.IsNullOrWhiteSpace(reason))return Result.Failure(OffboardingErrors.ReasonRequired);
        Status=OffboardingStatus.Cancelled;CancellationReason=reason.Trim();CancelledAtUtc=now.ToUniversalTime();CancelledByUserId=actor;SetModifiedAudit(now,actor);return Result.Success();
    }
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private void RecalculateStatus(){if(Status is OffboardingStatus.Completed or OffboardingStatus.Cancelled)return;Status=_items.All(x=>x.Status is OffboardingChecklistItemStatus.Completed or OffboardingChecklistItemStatus.Waived)?OffboardingStatus.ReadyToComplete:OffboardingStatus.InProgress;}
    private static bool IsAutomatic(OffboardingChecklistItemKind kind)=>kind is OffboardingChecklistItemKind.BranchAssignmentsClosed or OffboardingChecklistItemKind.JobPositionsClosed or OffboardingChecklistItemKind.EmploymentContractsClosed or OffboardingChecklistItemKind.EquipmentReturned or OffboardingChecklistItemKind.TimesheetsFinalized or OffboardingChecklistItemKind.ProfessionalRestrictionsReviewed;
}

public sealed class OffboardingChecklistItem
{
    private OffboardingChecklistItem(){}
    private OffboardingChecklistItem(OffboardingChecklistItemId id,OffboardingChecklistItemKind kind,bool automatic){Id=id;Kind=kind;IsAutomatic=automatic;Status=OffboardingChecklistItemStatus.Pending;}
    public OffboardingChecklistItemId Id{get;private set;}
    public OffboardingChecklistItemKind Kind{get;private set;}
    public bool IsAutomatic{get;private set;}
    public OffboardingChecklistItemStatus Status{get;private set;}
    public int BlockerCount{get;private set;}
    public string? Note{get;private set;}
    public DateTimeOffset? ResolvedAtUtc{get;private set;}
    public UserId? ResolvedByUserId{get;private set;}
    public string? WaiverReason{get;private set;}
    public DateTimeOffset? LastEvaluatedAtUtc{get;private set;}
    internal static OffboardingChecklistItem Create(OffboardingChecklistItemId id,OffboardingChecklistItemKind kind,bool automatic)=>new(id,kind,automatic);
    internal void Synchronize(int blockers,string? note,DateTimeOffset now,UserId actor){BlockerCount=Math.Max(0,blockers);Note=Norm(note);LastEvaluatedAtUtc=now.ToUniversalTime();WaiverReason=null;if(BlockerCount==0){Status=OffboardingChecklistItemStatus.Completed;ResolvedAtUtc=now.ToUniversalTime();ResolvedByUserId=actor;}else{Status=OffboardingChecklistItemStatus.Pending;ResolvedAtUtc=null;ResolvedByUserId=null;}}
    internal void Complete(string? note,DateTimeOffset now,UserId actor){BlockerCount=0;Note=Norm(note);WaiverReason=null;Status=OffboardingChecklistItemStatus.Completed;ResolvedAtUtc=now.ToUniversalTime();ResolvedByUserId=actor;}
    internal void Waive(string reason,DateTimeOffset now,UserId actor){WaiverReason=reason.Trim();Status=OffboardingChecklistItemStatus.Waived;ResolvedAtUtc=now.ToUniversalTime();ResolvedByUserId=actor;}
    private static string? Norm(string? s)=>string.IsNullOrWhiteSpace(s)?null:s.Trim();
}

public static class OffboardingErrors
{
    public static readonly Error InvalidIdentifier=Error.Validation("Workforce.Offboarding.InvalidIdentifier","errors.workforce.offboarding.invalidIdentifier");
    public static readonly Error ReasonRequired=Error.Validation("Workforce.Offboarding.ReasonRequired","errors.workforce.offboarding.reasonRequired");
    public static readonly Error WaiverReasonRequired=Error.Validation("Workforce.Offboarding.WaiverReasonRequired","errors.workforce.offboarding.waiverReasonRequired");
    public static readonly Error NotFound=Error.NotFound("Workforce.Offboarding.NotFound","errors.workforce.offboarding.notFound");
    public static readonly Error ExistingProcess=Error.Conflict("Workforce.Offboarding.ExistingProcess","errors.workforce.offboarding.existingProcess");
    public static readonly Error EmployeeMustBeEnding=Error.Conflict("Workforce.Offboarding.EmployeeMustBeEnding","errors.workforce.offboarding.employeeMustBeEnding");
    public static readonly Error ChecklistIncomplete=Error.Conflict("Workforce.Offboarding.ChecklistIncomplete","errors.workforce.offboarding.checklistIncomplete");
    public static readonly Error NotEditable=Error.Conflict("Workforce.Offboarding.NotEditable","errors.workforce.offboarding.notEditable");
    public static readonly Error InvalidTransition=Error.Conflict("Workforce.Offboarding.InvalidTransition","errors.workforce.offboarding.invalidTransition");
    public static readonly Error ItemNotFound=Error.NotFound("Workforce.Offboarding.ItemNotFound","errors.workforce.offboarding.itemNotFound");
    public static readonly Error ManualItemExpected=Error.Conflict("Workforce.Offboarding.ManualItemExpected","errors.workforce.offboarding.manualItemExpected");
    public static readonly Error AutomaticItemExpected=Error.Conflict("Workforce.Offboarding.AutomaticItemExpected","errors.workforce.offboarding.automaticItemExpected");
}

public interface IOffboardingProcessRepository
{
    Task<OffboardingProcess?> GetAsync(OrganizationId organizationId,OffboardingProcessId id,bool tracking,CancellationToken ct=default);
    Task<OffboardingProcess?> FindCurrentByEmployeeAsync(OrganizationId organizationId,EmployeeId employeeId,bool tracking,CancellationToken ct=default);
    void Add(OffboardingProcess process);
}
