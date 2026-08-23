using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Workforce.Domain.BranchAssignments;

public sealed class EmployeeBranchAssignment : Entity<EmployeeBranchAssignmentId>
{
    private EmployeeBranchAssignment() { }

    private EmployeeBranchAssignment(
        EmployeeBranchAssignmentId id,
        BranchId branchId,
        DateOnly startDate,
        DateOnly? endDate,
        bool isPrimary,
        EmployeeBranchAssignmentStatus status,
        DateTimeOffset nowUtc,
        UserId actorUserId) : base(id)
    {
        BranchId = branchId;
        StartDate = startDate;
        EndDate = endDate;
        IsPrimary = isPrimary;
        Status = status;
        CreatedAtUtc = nowUtc.ToUniversalTime();
        CreatedByUserId = actorUserId;
    }

    public BranchId BranchId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsPrimary { get; private set; }
    public EmployeeBranchAssignmentStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    internal static Result<EmployeeBranchAssignment> Create(
        EmployeeBranchAssignmentId id,
        BranchId branchId,
        DateOnly startDate,
        DateOnly? endDate,
        bool isPrimary,
        DateOnly today,
        DateTimeOffset nowUtc,
        UserId actorUserId)
    {
        if (id.IsEmpty) return Result.Failure<EmployeeBranchAssignment>(EmployeeBranchAssignmentErrors.InvalidIdentifier);
        if (branchId.IsEmpty) return Result.Failure<EmployeeBranchAssignment>(EmployeeBranchAssignmentErrors.BranchRequired);
        if (endDate.HasValue && endDate.Value < startDate) return Result.Failure<EmployeeBranchAssignment>(EmployeeBranchAssignmentErrors.InvalidPeriod);

        EmployeeBranchAssignmentStatus status = startDate > today
            ? EmployeeBranchAssignmentStatus.Planned
            : endDate.HasValue && endDate.Value < today
                ? EmployeeBranchAssignmentStatus.Ended
                : EmployeeBranchAssignmentStatus.Active;

        return Result.Success(new EmployeeBranchAssignment(id, branchId, startDate, endDate, isPrimary, status, nowUtc, actorUserId));
    }

    internal Result Update(DateOnly startDate, DateOnly? endDate, bool isPrimary, DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status is EmployeeBranchAssignmentStatus.Cancelled or EmployeeBranchAssignmentStatus.Ended)
            return Result.Failure(EmployeeBranchAssignmentErrors.NotEditable);
        if (endDate.HasValue && endDate.Value < startDate)
            return Result.Failure(EmployeeBranchAssignmentErrors.InvalidPeriod);

        StartDate = startDate;
        EndDate = endDate;
        IsPrimary = isPrimary;
        Status = startDate > today
            ? EmployeeBranchAssignmentStatus.Planned
            : endDate.HasValue && endDate.Value < today
                ? EmployeeBranchAssignmentStatus.Ended
                : EmployeeBranchAssignmentStatus.Active;
        SetModified(nowUtc, actorUserId);
        return Result.Success();
    }

    internal Result End(DateOnly endDate, DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status == EmployeeBranchAssignmentStatus.Cancelled)
            return Result.Failure(EmployeeBranchAssignmentErrors.InvalidStatusTransition);
        if (endDate < StartDate)
            return Result.Failure(EmployeeBranchAssignmentErrors.InvalidPeriod);

        EndDate = endDate;
        Status = endDate < today ? EmployeeBranchAssignmentStatus.Ended : EmployeeBranchAssignmentStatus.Active;
        SetModified(nowUtc, actorUserId);
        return Result.Success();
    }

    internal Result Cancel(DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != EmployeeBranchAssignmentStatus.Planned)
            return Result.Failure(EmployeeBranchAssignmentErrors.InvalidStatusTransition);

        Status = EmployeeBranchAssignmentStatus.Cancelled;
        SetModified(nowUtc, actorUserId);
        return Result.Success();
    }

    private void SetModified(DateTimeOffset nowUtc, UserId actorUserId)
    {
        LastModifiedAtUtc = nowUtc.ToUniversalTime();
        LastModifiedByUserId = actorUserId;
    }
}
