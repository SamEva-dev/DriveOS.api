using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.WorkingTime;
public enum WorkingTimePolicyStatus { Active=1, Inactive=2 }
/// <summary>HR-owned contractual working-time frame. It does not own planned bookings or executed sessions.</summary>
public sealed class WorkingTimePolicy : AggregateRoot<WorkingTimePolicyId>, IAuditableEntity
{
    private WorkingTimePolicy() { }
    private WorkingTimePolicy(WorkingTimePolicyId id, OrganizationId organizationId, EmployeeId employeeId, DateOnly effectiveFrom, DateOnly? effectiveTo, decimal contractualWeeklyHours, decimal? contractualDailyHours, int? maxWorkingDaysPerWeek) : base(id)
    { OrganizationId=organizationId; EmployeeId=employeeId; EffectiveFrom=effectiveFrom; EffectiveTo=effectiveTo; ContractualWeeklyHours=contractualWeeklyHours; ContractualDailyHours=contractualDailyHours; MaxWorkingDaysPerWeek=maxWorkingDaysPerWeek; Status=WorkingTimePolicyStatus.Active; }
    public OrganizationId OrganizationId { get; private set; }
    public EmployeeId EmployeeId { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public decimal ContractualWeeklyHours { get; private set; }
    public decimal? ContractualDailyHours { get; private set; }
    public int? MaxWorkingDaysPerWeek { get; private set; }
    public WorkingTimePolicyStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }
    public static Result<WorkingTimePolicy> Create(WorkingTimePolicyId id, OrganizationId org, EmployeeId employeeId, DateOnly from, DateOnly? to, decimal weekly, decimal? daily, int? maxDays)
    {
        if(id.IsEmpty||org.IsEmpty||employeeId.IsEmpty) return Result.Failure<WorkingTimePolicy>(WorkingTimeErrors.InvalidIdentifier);
        if(to.HasValue&&to.Value<from) return Result.Failure<WorkingTimePolicy>(WorkingTimeErrors.InvalidPeriod);
        if(weekly<=0||weekly>168||daily is <=0 or >24||maxDays is <1 or >7) return Result.Failure<WorkingTimePolicy>(WorkingTimeErrors.InvalidLimits);
        return Result.Success(new WorkingTimePolicy(id,org,employeeId,from,to,weekly,daily,maxDays));
    }
    public Result Update(DateOnly from,DateOnly? to,decimal weekly,decimal? daily,int? maxDays,DateTimeOffset now,UserId actor){if(Status!=WorkingTimePolicyStatus.Active)return Result.Failure(WorkingTimeErrors.Inactive);if(to.HasValue&&to.Value<from)return Result.Failure(WorkingTimeErrors.InvalidPeriod);if(weekly<=0||weekly>168||daily is <=0 or >24||maxDays is <1 or >7)return Result.Failure(WorkingTimeErrors.InvalidLimits);EffectiveFrom=from;EffectiveTo=to;ContractualWeeklyHours=weekly;ContractualDailyHours=daily;MaxWorkingDaysPerWeek=maxDays;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Deactivate(DateTimeOffset now,UserId actor){if(Status==WorkingTimePolicyStatus.Inactive)return Result.Failure(WorkingTimeErrors.Inactive);Status=WorkingTimePolicyStatus.Inactive;SetModifiedAudit(now,actor);return Result.Success();}
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
}
public static class WorkingTimeErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Workforce.WorkingTime.InvalidIdentifier", "workforce.workingTime.errors.invalidIdentifier");
    public static readonly Error InvalidPeriod = Error.Validation("Workforce.WorkingTime.InvalidPeriod", "workforce.workingTime.errors.invalidPeriod");
    public static readonly Error InvalidLimits = Error.Validation("Workforce.WorkingTime.InvalidLimits", "workforce.workingTime.errors.invalidLimits");
    public static readonly Error OverlappingPolicy = Error.Conflict("Workforce.WorkingTime.OverlappingPolicy", "workforce.workingTime.errors.overlappingPolicy");
    public static readonly Error NotFound = Error.NotFound("Workforce.WorkingTime.NotFound", "workforce.workingTime.errors.notFound");
    public static readonly Error Inactive = Error.Conflict("Workforce.WorkingTime.Inactive", "workforce.workingTime.errors.inactive");
}
public interface IWorkingTimePolicyRepository
{
    Task<WorkingTimePolicy?> GetAsync(OrganizationId organizationId,WorkingTimePolicyId id,bool tracking,CancellationToken ct=default);
    Task<IReadOnlyList<WorkingTimePolicy>> ListAsync(OrganizationId organizationId,EmployeeId employeeId,CancellationToken ct=default);
    Task<bool> HasOverlapAsync(OrganizationId organizationId,EmployeeId employeeId,DateOnly from,DateOnly? to,WorkingTimePolicyId? excluding,CancellationToken ct=default);
    void Add(WorkingTimePolicy policy);
}
