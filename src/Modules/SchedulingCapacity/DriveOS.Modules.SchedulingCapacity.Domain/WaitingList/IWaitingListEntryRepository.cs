using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;

public interface IWaitingListEntryRepository
{
    Task<WaitingListEntry?> GetByIdForUpdateAsync(WaitingListEntryId id, OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<WaitingListEntry>> GetCandidatesAsync(OrganizationId organizationId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, CancellationToken cancellationToken = default);
    Task<bool> HasActiveHoldAsync(OrganizationId organizationId, string slotKey, WaitingListEntryId exceptEntryId, CancellationToken cancellationToken = default);
    void Add(WaitingListEntry entry);
}
