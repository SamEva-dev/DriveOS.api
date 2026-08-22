using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Places.Watch;

public interface IExamPlaceWatchRepository
{
    Task<ExamPlaceWatchSubscription?> GetByIdAsync(OrganizationId organizationId, ExamPlaceWatchSubscriptionId id, CancellationToken cancellationToken = default);
    Task<ExamPlaceWatchSubscription?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamPlaceWatchSubscriptionId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamPlaceWatchSubscription>> ListAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamPlaceWatchSubscription>> ClaimDueAsync(DateTimeOffset nowUtc, int take, TimeSpan leaseDuration, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamPlaceWatchScan>> ListScansAsync(OrganizationId organizationId, ExamPlaceWatchSubscriptionId subscriptionId, int take, CancellationToken cancellationToken = default);
    Task<bool> HitExistsAsync(ExamPlaceWatchSubscriptionId subscriptionId, ExamPlaceId examPlaceId, CancellationToken cancellationToken = default);
    void Add(ExamPlaceWatchSubscription subscription);
    void Add(ExamPlaceWatchScan scan);
    void Add(ExamPlaceWatchHit hit);
}
