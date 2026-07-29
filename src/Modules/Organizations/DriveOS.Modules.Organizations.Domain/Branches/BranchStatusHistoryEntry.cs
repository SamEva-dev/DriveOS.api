using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Branches;

public sealed class BranchStatusHistoryEntry
{
    private BranchStatusHistoryEntry()
    {
    }

    private BranchStatusHistoryEntry(
        Guid id,
        BranchId branchId,
        BranchStatus previousStatus,
        BranchStatus newStatus,
        BranchStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc)
    {
        Id = id;
        BranchId = branchId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason;
        ChangedByUserId = changedByUserId;
        ChangedAtUtc = changedAtUtc;
    }

    public Guid Id { get; private set; }

    public BranchId BranchId { get; private set; }

    public BranchStatus PreviousStatus
    {
        get;
        private set;
    }

    public BranchStatus NewStatus
    {
        get;
        private set;
    }

    public BranchStatusChangeReason Reason
    {
        get;
        private set;
    } = null!;

    public Guid ChangedByUserId
    {
        get;
        private set;
    }

    public DateTimeOffset ChangedAtUtc
    {
        get;
        private set;
    }

    internal static BranchStatusHistoryEntry Create(
        BranchId branchId,
        BranchStatus previousStatus,
        BranchStatus newStatus,
        BranchStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc)
    {
        if (branchId.IsEmpty)
        {
            throw new ArgumentException(
                "The branch identifier is required.",
                nameof(branchId));
        }

        if (previousStatus == newStatus)
        {
            throw new InvalidOperationException(
                "The previous and new statuses cannot be identical.");
        }

        if (changedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "The user identifier is required.",
                nameof(changedByUserId));
        }

        ArgumentNullException.ThrowIfNull(
            reason);

        return new BranchStatusHistoryEntry(
            Guid.NewGuid(),
            branchId,
            previousStatus,
            newStatus,
            reason,
            changedByUserId,
            changedAtUtc);
    }
}