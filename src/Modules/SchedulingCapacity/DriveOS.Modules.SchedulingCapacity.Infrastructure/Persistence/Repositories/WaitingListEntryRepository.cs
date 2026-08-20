using DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Repositories;

internal sealed class WaitingListEntryRepository(SchedulingCapacityDbContext dbContext) : IWaitingListEntryRepository
{
    public Task<WaitingListEntry?> GetByIdForUpdateAsync(WaitingListEntryId id, OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        dbContext.WaitingListEntries.Include(x => x.Proposals).SingleOrDefaultAsync(x => x.Id == id && x.OrganizationId == organizationId, cancellationToken);

    public async Task<IReadOnlyCollection<WaitingListEntry>> GetCandidatesAsync(OrganizationId organizationId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, CancellationToken cancellationToken = default) =>
        await dbContext.WaitingListEntries.Include(x => x.Proposals)
            .Where(x => x.OrganizationId == organizationId && x.ExpiresAtUtc > DateTimeOffset.UtcNow &&
                (x.Status == WaitingListStatus.Waiting || x.Status == WaitingListStatus.Proposed || x.Status == WaitingListStatus.Declined) &&
                x.PreferredFromUtc <= startAtUtc && x.PreferredToUtc >= endAtUtc)
            .ToArrayAsync(cancellationToken);

    public Task<bool> HasActiveHoldAsync(OrganizationId organizationId, string slotKey, WaitingListEntryId exceptEntryId, CancellationToken cancellationToken = default) =>
        dbContext.WaitingListProposals.AnyAsync(x => x.OrganizationId == organizationId && x.WaitingListEntryId != exceptEntryId &&
            x.ActiveHoldKey == slotKey && x.Status == WaitingListProposalStatus.TemporarilyHeld && x.HeldUntilUtc > DateTimeOffset.UtcNow, cancellationToken);

    public void Add(WaitingListEntry entry) => dbContext.WaitingListEntries.Add(entry);
}
