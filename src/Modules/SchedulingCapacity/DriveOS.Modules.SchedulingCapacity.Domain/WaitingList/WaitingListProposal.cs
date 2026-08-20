using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;

public sealed class WaitingListProposal : Entity<WaitingListProposalId>
{
    private WaitingListProposal() { }

    internal WaitingListProposal(
        WaitingListProposalId id,
        WaitingListEntryId entryId,
        OrganizationId organizationId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        BranchId? branchId,
        UserId? instructorId,
        DateTimeOffset expiresAtUtc) : base(id)
    {
        WaitingListEntryId = entryId;
        OrganizationId = organizationId;
        StartAtUtc = startAtUtc.ToUniversalTime();
        EndAtUtc = endAtUtc.ToUniversalTime();
        BranchId = branchId;
        InstructorId = instructorId;
        ExpiresAtUtc = expiresAtUtc.ToUniversalTime();
        Status = WaitingListProposalStatus.Proposed;
        ProposedAtUtc = DateTimeOffset.UtcNow;
        SlotKey = BuildSlotKey(organizationId, StartAtUtc, EndAtUtc, branchId, instructorId);
    }

    public WaitingListEntryId WaitingListEntryId { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public DateTimeOffset StartAtUtc { get; private set; }
    public DateTimeOffset EndAtUtc { get; private set; }
    public BranchId? BranchId { get; private set; }
    public UserId? InstructorId { get; private set; }
    public DateTimeOffset ProposedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public WaitingListProposalStatus Status { get; private set; }
    public string SlotKey { get; private set; } = string.Empty;
    public string? ActiveHoldKey { get; private set; }
    public DateTimeOffset? HeldUntilUtc { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
    public string? DecisionReason { get; private set; }
    public BookingId? FulfilledBookingId { get; private set; }

    internal Result Hold(DateTimeOffset heldUntilUtc)
    {
        if (Status != WaitingListProposalStatus.Proposed) return Result.Failure(WaitingListErrors.ProposalClosed);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset until = heldUntilUtc.ToUniversalTime();
        if (ExpiresAtUtc <= now || until <= now || until > ExpiresAtUtc) return Result.Failure(WaitingListErrors.HoldExpired);
        Status = WaitingListProposalStatus.TemporarilyHeld;
        HeldUntilUtc = until;
        ActiveHoldKey = SlotKey;
        return Result.Success();
    }

    internal Result Accept()
    {
        if (Status is not (WaitingListProposalStatus.Proposed or WaitingListProposalStatus.TemporarilyHeld)) return Result.Failure(WaitingListErrors.ProposalClosed);
        if (ExpiresAtUtc <= DateTimeOffset.UtcNow || (Status == WaitingListProposalStatus.TemporarilyHeld && HeldUntilUtc.HasValue && HeldUntilUtc.Value <= DateTimeOffset.UtcNow)) return Result.Failure(WaitingListErrors.HoldExpired);
        Status = WaitingListProposalStatus.Accepted;
        ActiveHoldKey = null;
        DecidedAtUtc = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    internal Result Fulfill(BookingId bookingId)
    {
        if (Status != WaitingListProposalStatus.Accepted || bookingId.IsEmpty) return Result.Failure(WaitingListErrors.ProposalClosed);
        FulfilledBookingId = bookingId;
        return Result.Success();
    }

    internal Result Decline(string? reason)
    {
        if (Status is not (WaitingListProposalStatus.Proposed or WaitingListProposalStatus.TemporarilyHeld)) return Result.Failure(WaitingListErrors.ProposalClosed);
        Status = WaitingListProposalStatus.Declined;
        ActiveHoldKey = null;
        DecidedAtUtc = DateTimeOffset.UtcNow;
        DecisionReason = Normalize(reason, 500);
        return Result.Success();
    }

    internal void ExpireIfNeeded(DateTimeOffset nowUtc)
    {
        if ((Status is WaitingListProposalStatus.Proposed or WaitingListProposalStatus.TemporarilyHeld) &&
            (ExpiresAtUtc <= nowUtc || (HeldUntilUtc.HasValue && HeldUntilUtc.Value <= nowUtc)))
        {
            Status = WaitingListProposalStatus.Expired;
            ActiveHoldKey = null;
            DecidedAtUtc ??= nowUtc;
        }
    }

    private static string BuildSlotKey(OrganizationId organizationId, DateTimeOffset start, DateTimeOffset end, BranchId? branchId, UserId? instructorId) =>
        $"{organizationId.Value:N}:{start.UtcDateTime.Ticks}:{end.UtcDateTime.Ticks}:{branchId?.Value:N}:{instructorId?.Value:N}";

    private static string? Normalize(string? value, int maxLength)
    {
        string normalized = value?.Trim() ?? string.Empty;
        return normalized.Length == 0 ? null : normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
