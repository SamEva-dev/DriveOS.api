using DriveOS.Modules.SchedulingCapacity.Application.WaitingList;
using DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class WaitingListReadService(SchedulingCapacityDbContext dbContext) : IWaitingListReadService
{
    public async Task<IReadOnlyCollection<WaitingListEntryResponse>> ListAsync(
        OrganizationId organizationId,
        int? status,
        Guid? studentId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<WaitingListEntry> query = dbContext.WaitingListEntries
            .AsNoTracking()
            .Include(x => x.Proposals)
            .Where(x => x.OrganizationId == organizationId);

        if (studentId.HasValue)
            query = query.Where(x => x.StudentId == new PersonId(studentId.Value));

        WaitingListEntry[] entries = await query.ToArrayAsync(cancellationToken);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        IEnumerable<WaitingListEntryResponse> mapped = entries
            .OrderByDescending(x => WaitingListPriorityPolicy.CalculateEffectiveScore(x.PriorityScore, x.CreatedAtUtc, now))
            .ThenBy(x => x.CreatedAtUtc)
            .Select(x => Map(x, now));

        if (status.HasValue && Enum.IsDefined(typeof(WaitingListStatus), status.Value))
            mapped = mapped.Where(x => x.Status == status.Value);

        return mapped.ToArray();
    }

    public async Task<WaitingListEntryResponse?> GetAsync(
        OrganizationId organizationId,
        WaitingListEntryId entryId,
        CancellationToken cancellationToken = default)
    {
        WaitingListEntry? entry = await dbContext.WaitingListEntries
            .AsNoTracking()
            .Include(x => x.Proposals)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == entryId, cancellationToken);

        return entry is null ? null : Map(entry, DateTimeOffset.UtcNow);
    }

    private static WaitingListEntryResponse Map(WaitingListEntry entry, DateTimeOffset nowUtc)
    {
        WaitingListProposalResponse[] proposals = entry.Proposals
            .OrderByDescending(x => x.ProposedAtUtc)
            .Select(x => new WaitingListProposalResponse(
                x.Id.Value,
                x.StartAtUtc,
                x.EndAtUtc,
                x.BranchId?.Value,
                x.InstructorId?.Value,
                x.ProposedAtUtc,
                x.ExpiresAtUtc,
                (int)EffectiveProposalStatus(x, nowUtc),
                x.HeldUntilUtc,
                x.DecidedAtUtc,
                x.DecisionReason,
                x.FulfilledBookingId?.Value))
            .ToArray();

        WaitingListStatus effectiveStatus = EffectiveEntryStatus(entry, proposals, nowUtc);

        return new WaitingListEntryResponse(
            entry.Id.Value,
            entry.StudentId.Value,
            (int)entry.RequestedSessionType,
            entry.PreferredFromUtc,
            entry.PreferredToUtc,
            entry.DurationMinutes,
            entry.PreferredBranchId?.Value,
            entry.PreferredInstructorId?.Value,
            entry.PriorityScore,
            WaitingListPriorityPolicy.CalculateEffectiveScore(entry.PriorityScore, entry.CreatedAtUtc, nowUtc),
            entry.PriorityExplanation,
            entry.Reason,
            entry.CreatedAtUtc,
            entry.ExpiresAtUtc,
            (int)effectiveStatus,
            proposals);
    }

    private static WaitingListStatus EffectiveEntryStatus(
        WaitingListEntry entry,
        IReadOnlyCollection<WaitingListProposalResponse> proposals,
        DateTimeOffset nowUtc)
    {
        if (entry.Status is WaitingListStatus.Cancelled or WaitingListStatus.Fulfilled)
            return entry.Status;

        if (entry.ExpiresAtUtc <= nowUtc)
            return WaitingListStatus.Expired;

        if (entry.Status == WaitingListStatus.TemporarilyHeld && proposals.All(x => x.Status != (int)WaitingListProposalStatus.TemporarilyHeld))
            return WaitingListStatus.Waiting;

        return entry.Status;
    }

    private static WaitingListProposalStatus EffectiveProposalStatus(WaitingListProposal proposal, DateTimeOffset nowUtc)
    {
        if (proposal.Status is WaitingListProposalStatus.Proposed or WaitingListProposalStatus.TemporarilyHeld)
        {
            bool proposalExpired = proposal.ExpiresAtUtc <= nowUtc;
            bool holdExpired = proposal.Status == WaitingListProposalStatus.TemporarilyHeld
                               && proposal.HeldUntilUtc.HasValue
                               && proposal.HeldUntilUtc.Value <= nowUtc;

            if (proposalExpired || holdExpired)
                return WaitingListProposalStatus.Expired;
        }

        return proposal.Status;
    }
}
