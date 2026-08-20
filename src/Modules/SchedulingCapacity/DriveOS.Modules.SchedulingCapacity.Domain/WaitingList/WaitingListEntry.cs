using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.WaitingList.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;

public sealed class WaitingListEntry : AggregateRoot<WaitingListEntryId>, IAuditableEntity
{
    private readonly List<WaitingListProposal> proposals = [];
    private WaitingListEntry() { }

    private WaitingListEntry(
        WaitingListEntryId id,
        OrganizationId organizationId,
        PersonId studentId,
        BookingType requestedSessionType,
        DateTimeOffset preferredFromUtc,
        DateTimeOffset preferredToUtc,
        int durationMinutes,
        BranchId? preferredBranchId,
        UserId? preferredUserId,
        int priorityScore,
        string priorityExplanation,
        string reason,
        DateTimeOffset expiresAtUtc) : base(id)
    {
        OrganizationId = organizationId;
        StudentId = studentId;
        RequestedSessionType = requestedSessionType;
        PreferredFromUtc = preferredFromUtc.ToUniversalTime();
        PreferredToUtc = preferredToUtc.ToUniversalTime();
        DurationMinutes = durationMinutes;
        PreferredBranchId = preferredBranchId;
        PreferredInstructorId = preferredUserId;
        PriorityScore = priorityScore;
        PriorityExplanation = priorityExplanation;
        Reason = reason;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        ExpiresAtUtc = expiresAtUtc.ToUniversalTime();
        Status = WaitingListStatus.Waiting;
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public BookingType RequestedSessionType { get; private set; }
    public DateTimeOffset PreferredFromUtc { get; private set; }
    public DateTimeOffset PreferredToUtc { get; private set; }
    public int DurationMinutes { get; private set; }
    public BranchId? PreferredBranchId { get; private set; }
    public UserId? PreferredInstructorId { get; private set; }
    public int PriorityScore { get; private set; }
    public string PriorityExplanation { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public WaitingListStatus Status { get; private set; }
    public IReadOnlyCollection<WaitingListProposal> Proposals => proposals.AsReadOnly();
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<WaitingListEntry> Create(
        WaitingListEntryId id,
        OrganizationId organizationId,
        PersonId studentId,
        BookingType requestedSessionType,
        DateTimeOffset preferredFromUtc,
        DateTimeOffset preferredToUtc,
        int durationMinutes,
        BranchId? preferredBranchId,
        UserId? preferredUserId,
        int priorityScore,
        string priorityExplanation,
        string reason,
        DateTimeOffset expiresAtUtc)
    {
        if (id.IsEmpty || organizationId.IsEmpty || studentId.IsEmpty) return Result.Failure<WaitingListEntry>(WaitingListErrors.InvalidIdentifier);
        if (!Enum.IsDefined(requestedSessionType)) return Result.Failure<WaitingListEntry>(WaitingListErrors.InvalidReason);
        DateTimeOffset from = preferredFromUtc.ToUniversalTime();
        DateTimeOffset to = preferredToUtc.ToUniversalTime();
        DateTimeOffset expiry = expiresAtUtc.ToUniversalTime();
        if (to <= from) return Result.Failure<WaitingListEntry>(WaitingListErrors.InvalidPeriod);
        if (durationMinutes is < 15 or > 720 || TimeSpan.FromMinutes(durationMinutes) > to - from) return Result.Failure<WaitingListEntry>(WaitingListErrors.InvalidDuration);
        if (priorityScore is < 0 or > 100 || string.IsNullOrWhiteSpace(priorityExplanation)) return Result.Failure<WaitingListEntry>(WaitingListErrors.InvalidPriority);
        if (string.IsNullOrWhiteSpace(reason)) return Result.Failure<WaitingListEntry>(WaitingListErrors.InvalidReason);
        if (expiry <= DateTimeOffset.UtcNow) return Result.Failure<WaitingListEntry>(WaitingListErrors.InvalidExpiration);

        var entry = new WaitingListEntry(id, organizationId, studentId, requestedSessionType, from, to, durationMinutes, preferredBranchId, preferredUserId,
            priorityScore, Normalize(priorityExplanation, 500), Normalize(reason, 1000), expiry);
        entry.RaiseDomainEvent(new WaitingListEntryCreatedDomainEvent(id, organizationId, studentId));
        return Result.Success(entry);
    }

    public Result UpdatePreferences(DateTimeOffset preferredFromUtc, DateTimeOffset preferredToUtc, BranchId? branchId, UserId? instructorId, DateTimeOffset expiresAtUtc)
    {
        if (Status is WaitingListStatus.Cancelled or WaitingListStatus.Fulfilled or WaitingListStatus.Expired) return Result.Failure(WaitingListErrors.NotWaiting);
        DateTimeOffset from = preferredFromUtc.ToUniversalTime();
        DateTimeOffset to = preferredToUtc.ToUniversalTime();
        DateTimeOffset expiry = expiresAtUtc.ToUniversalTime();
        if (to <= from || TimeSpan.FromMinutes(DurationMinutes) > to - from) return Result.Failure(WaitingListErrors.InvalidPeriod);
        if (expiry <= DateTimeOffset.UtcNow) return Result.Failure(WaitingListErrors.InvalidExpiration);
        PreferredFromUtc = from;
        PreferredToUtc = to;
        PreferredBranchId = branchId;
        PreferredInstructorId = instructorId;
        ExpiresAtUtc = expiry;
        return Result.Success();
    }

    public bool Matches(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, BranchId? branchId, UserId? instructorId, DateTimeOffset nowUtc)
    {
        if (Status is not (WaitingListStatus.Waiting or WaitingListStatus.Proposed or WaitingListStatus.Declined)) return false;
        if (ExpiresAtUtc <= nowUtc) return false;
        DateTimeOffset start = startAtUtc.ToUniversalTime();
        DateTimeOffset end = endAtUtc.ToUniversalTime();
        if (start < PreferredFromUtc || end > PreferredToUtc || end <= start) return false;
        if ((end - start).TotalMinutes < DurationMinutes) return false;
        if (PreferredBranchId.HasValue && PreferredBranchId != branchId) return false;
        if (PreferredInstructorId.HasValue && PreferredInstructorId != instructorId) return false;
        return true;
    }

    public Result<WaitingListProposalId> Propose(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, BranchId? branchId, UserId? instructorId, DateTimeOffset expiresAtUtc)
    {
        if (!Matches(startAtUtc, endAtUtc, branchId, instructorId, DateTimeOffset.UtcNow)) return Result.Failure<WaitingListProposalId>(WaitingListErrors.InvalidPeriod);
        DateTimeOffset expiry = expiresAtUtc.ToUniversalTime();
        if (expiry <= DateTimeOffset.UtcNow || expiry > ExpiresAtUtc) return Result.Failure<WaitingListProposalId>(WaitingListErrors.InvalidExpiration);
        var id = WaitingListProposalId.New();
        proposals.Add(new WaitingListProposal(id, Id, OrganizationId, startAtUtc, endAtUtc, branchId, instructorId, expiry));
        Status = WaitingListStatus.Proposed;
        RaiseDomainEvent(new WaitingListSlotProposedDomainEvent(Id, OrganizationId, id, startAtUtc.ToUniversalTime(), expiry));
        return Result.Success(id);
    }

    public Result Hold(WaitingListProposalId proposalId, DateTimeOffset heldUntilUtc)
    {
        WaitingListProposal? proposal = proposals.SingleOrDefault(x => x.Id == proposalId);
        if (proposal is null) return Result.Failure(WaitingListErrors.ProposalNotFound);
        Result result = proposal.Hold(heldUntilUtc);
        if (result.IsFailure) return result;
        Status = WaitingListStatus.TemporarilyHeld;
        return Result.Success();
    }

    public Result Accept(WaitingListProposalId proposalId)
    {
        WaitingListProposal? proposal = proposals.SingleOrDefault(x => x.Id == proposalId);
        if (proposal is null) return Result.Failure(WaitingListErrors.ProposalNotFound);
        Result result = proposal.Accept();
        if (result.IsFailure) return result;
        Status = WaitingListStatus.Accepted;
        return Result.Success();
    }

    public Result Fulfill(WaitingListProposalId proposalId, BookingId bookingId)
    {
        WaitingListProposal? proposal = proposals.SingleOrDefault(x => x.Id == proposalId);
        if (proposal is null) return Result.Failure(WaitingListErrors.ProposalNotFound);
        Result result = proposal.Fulfill(bookingId);
        if (result.IsFailure) return result;
        Status = WaitingListStatus.Fulfilled;
        RaiseDomainEvent(new WaitingListEntryFulfilledDomainEvent(Id, OrganizationId, bookingId));
        return Result.Success();
    }

    public Result Decline(WaitingListProposalId proposalId, string? reason)
    {
        WaitingListProposal? proposal = proposals.SingleOrDefault(x => x.Id == proposalId);
        if (proposal is null) return Result.Failure(WaitingListErrors.ProposalNotFound);
        Result result = proposal.Decline(reason);
        if (result.IsFailure) return result;
        Status = WaitingListStatus.Declined;
        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status is WaitingListStatus.Cancelled or WaitingListStatus.Fulfilled or WaitingListStatus.Expired) return Result.Failure(WaitingListErrors.NotWaiting);
        if (string.IsNullOrWhiteSpace(reason)) return Result.Failure(WaitingListErrors.InvalidReason);
        Status = WaitingListStatus.Cancelled;
        return Result.Success();
    }

    public void ExpireIfNeeded(DateTimeOffset nowUtc)
    {
        foreach (WaitingListProposal proposal in proposals) proposal.ExpireIfNeeded(nowUtc);
        if (ExpiresAtUtc <= nowUtc && Status is not (WaitingListStatus.Cancelled or WaitingListStatus.Fulfilled)) Status = WaitingListStatus.Expired;
        else if (Status == WaitingListStatus.TemporarilyHeld && proposals.All(x => x.Status != WaitingListProposalStatus.TemporarilyHeld)) Status = WaitingListStatus.Waiting;
    }

    private static string Normalize(string value, int maxLength)
    {
        string normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId) { CreatedAtUtc = createdAtUtc.ToUniversalTime(); CreatedByUserId = createdByUserId; }
    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId) { LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime(); LastModifiedByUserId = modifiedByUserId; }
}
