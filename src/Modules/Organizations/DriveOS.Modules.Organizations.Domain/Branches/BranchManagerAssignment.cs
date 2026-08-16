using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Branches;

public sealed class BranchManagerAssignment : Entity<BranchManagerAssignmentId>
{
    private BranchManagerAssignment() { }

    private BranchManagerAssignment(
        BranchManagerAssignmentId id,
        BranchId branchId,
        UserId managerUserId,
        DateTimeOffset effectiveFromUtc,
        UserId assignedByUserId,
        DateTimeOffset assignedAtUtc
    )
        : base(id)
    {
        BranchId = branchId;
        ManagerUserId = managerUserId;
        EffectiveFromUtc = effectiveFromUtc;
        AssignedByUserId = assignedByUserId;
        AssignedAtUtc = assignedAtUtc;
        Status = BranchManagerAssignmentStatus.Active;
    }

    public BranchId BranchId { get; private set; }

    public UserId ManagerUserId { get; private set; }

    public DateTimeOffset EffectiveFromUtc { get; private set; }

    public DateTimeOffset? EffectiveToUtc { get; private set; }

    public BranchManagerAssignmentStatus Status { get; private set; }

    public UserId AssignedByUserId { get; private set; }

    public DateTimeOffset AssignedAtUtc { get; private set; }

    public UserId? EndedByUserId { get; private set; }

    public DateTimeOffset? EndedAtUtc { get; private set; }

    internal static BranchManagerAssignment Create(
        BranchId branchId,
        UserId managerUserId,
        DateTimeOffset effectiveFromUtc,
        UserId assignedByUserId,
        DateTimeOffset assignedAtUtc
    )
    {
        if (branchId.IsEmpty)
        {
            throw new ArgumentException("The branch identifier is required.", nameof(branchId));
        }

        if (managerUserId.IsEmpty)
        {
            throw new ArgumentException(
                "The manager user identifier is required.",
                nameof(managerUserId)
            );
        }

        if (assignedByUserId.IsEmpty)
        {
            throw new ArgumentException(
                "The assigning user identifier is required.",
                nameof(assignedByUserId)
            );
        }

        return new BranchManagerAssignment(
            BranchManagerAssignmentId.New(),
            branchId,
            managerUserId,
            effectiveFromUtc,
            assignedByUserId,
            assignedAtUtc
        );
    }

    internal void End(
        DateTimeOffset effectiveToUtc,
        UserId endedByUserId,
        DateTimeOffset endedAtUtc
    )
    {
        if (Status == BranchManagerAssignmentStatus.Ended)
        {
            return;
        }

        if (endedByUserId.IsEmpty)
        {
            throw new ArgumentException(
                "The ending user identifier is required.",
                nameof(endedByUserId)
            );
        }

        if (effectiveToUtc < EffectiveFromUtc)
        {
            throw new InvalidOperationException(
                "The assignment end date cannot precede its start date."
            );
        }

        EffectiveToUtc = effectiveToUtc;
        EndedByUserId = endedByUserId;
        EndedAtUtc = endedAtUtc;
        Status = BranchManagerAssignmentStatus.Ended;
    }

    public bool IsActiveAt(DateTimeOffset dateTimeUtc)
    {
        return Status == BranchManagerAssignmentStatus.Active
            && EffectiveFromUtc <= dateTimeUtc
            && (EffectiveToUtc is null || EffectiveToUtc > dateTimeUtc);
    }
}
