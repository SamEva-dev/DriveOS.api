using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.JobPositions;

public sealed class EmployeeJobPositionAssignment
{
    private EmployeeJobPositionAssignment() { }
    private EmployeeJobPositionAssignment(EmployeeJobPositionAssignmentId id, JobPositionId jobPositionId, BranchId? branchId, DateOnly startDate, DateOnly? endDate, bool isPrimary, DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        Id = id; JobPositionId = jobPositionId; BranchId = branchId; StartDate = startDate; EndDate = endDate; IsPrimary = isPrimary;
        Status = startDate > today ? EmployeeJobPositionAssignmentStatus.Planned : endDate.HasValue && endDate.Value < today ? EmployeeJobPositionAssignmentStatus.Ended : EmployeeJobPositionAssignmentStatus.Active;
        CreatedAtUtc = nowUtc.ToUniversalTime(); CreatedByUserId = actorUserId;
    }
    public EmployeeJobPositionAssignmentId Id { get; private set; }
    public JobPositionId JobPositionId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsPrimary { get; private set; }
    public EmployeeJobPositionAssignmentStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    internal static Result<EmployeeJobPositionAssignment> Create(EmployeeJobPositionAssignmentId id, JobPositionId jobPositionId, BranchId? branchId, DateOnly startDate, DateOnly? endDate, bool isPrimary, DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (id.IsEmpty || jobPositionId.IsEmpty) return Result.Failure<EmployeeJobPositionAssignment>(EmployeeJobPositionAssignmentErrors.InvalidIdentifier);
        if (endDate.HasValue && endDate.Value < startDate) return Result.Failure<EmployeeJobPositionAssignment>(EmployeeJobPositionAssignmentErrors.InvalidPeriod);
        return Result.Success(new EmployeeJobPositionAssignment(id, jobPositionId, branchId, startDate, endDate, isPrimary, today, nowUtc, actorUserId));
    }
    internal Result Update(DateOnly startDate, DateOnly? endDate, bool isPrimary, DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status is EmployeeJobPositionAssignmentStatus.Ended or EmployeeJobPositionAssignmentStatus.Cancelled) return Result.Failure(EmployeeJobPositionAssignmentErrors.InvalidTransition);
        if (endDate.HasValue && endDate.Value < startDate) return Result.Failure(EmployeeJobPositionAssignmentErrors.InvalidPeriod);
        StartDate = startDate; EndDate = endDate; IsPrimary = isPrimary; Status = startDate > today ? EmployeeJobPositionAssignmentStatus.Planned : endDate.HasValue && endDate.Value < today ? EmployeeJobPositionAssignmentStatus.Ended : EmployeeJobPositionAssignmentStatus.Active;
        LastModifiedAtUtc = nowUtc.ToUniversalTime(); LastModifiedByUserId = actorUserId; return Result.Success();
    }
    internal Result End(DateOnly endDate, DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status is EmployeeJobPositionAssignmentStatus.Ended or EmployeeJobPositionAssignmentStatus.Cancelled) return Result.Failure(EmployeeJobPositionAssignmentErrors.InvalidTransition);
        if (endDate < StartDate) return Result.Failure(EmployeeJobPositionAssignmentErrors.InvalidPeriod);
        EndDate = endDate; Status = endDate < today ? EmployeeJobPositionAssignmentStatus.Ended : EmployeeJobPositionAssignmentStatus.Active; LastModifiedAtUtc = nowUtc.ToUniversalTime(); LastModifiedByUserId = actorUserId; return Result.Success();
    }
    internal Result Cancel(DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != EmployeeJobPositionAssignmentStatus.Planned) return Result.Failure(EmployeeJobPositionAssignmentErrors.InvalidTransition);
        Status = EmployeeJobPositionAssignmentStatus.Cancelled; LastModifiedAtUtc = nowUtc.ToUniversalTime(); LastModifiedByUserId = actorUserId; return Result.Success();
    }
}
