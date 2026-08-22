using DriveOS.Modules.ExamsCertification.Domain.Places.Watch;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamPlaceWatchRepository(ExamsCertificationDbContext dbContext) : IExamPlaceWatchRepository
{
    public Task<ExamPlaceWatchSubscription?> GetByIdAsync(OrganizationId organizationId, ExamPlaceWatchSubscriptionId id, CancellationToken cancellationToken = default) =>
        dbContext.ExamPlaceWatchSubscriptions.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    public Task<ExamPlaceWatchSubscription?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamPlaceWatchSubscriptionId id, CancellationToken cancellationToken = default) =>
        dbContext.ExamPlaceWatchSubscriptions.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ExamPlaceWatchSubscription>> ListAsync(OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        await dbContext.ExamPlaceWatchSubscriptions.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.ProviderCode).ThenBy(x => x.ExamCategory).ThenBy(x => x.WindowFromUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ExamPlaceWatchSubscription>> ClaimDueAsync(DateTimeOffset nowUtc, int take, TimeSpan leaseDuration, CancellationToken cancellationToken = default)
    {
        int batchSize = Math.Clamp(take, 1, 100);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        List<ExamPlaceWatchSubscription> candidates = await dbContext.ExamPlaceWatchSubscriptions
            .FromSqlInterpolated($@"SELECT w.*, w.xmin FROM exams_certification.exam_place_watch_subscriptions AS w
                WHERE status = 'Active'
                  AND next_check_at_utc <= {nowUtc}
                  AND (processing_lease_until_utc IS NULL OR processing_lease_until_utc <= {nowUtc})
                ORDER BY next_check_at_utc
                FOR UPDATE SKIP LOCKED
                LIMIT {batchSize}")
            .ToListAsync(cancellationToken);

        foreach (ExamPlaceWatchSubscription subscription in candidates)
            subscription.TryAcquireProcessingLease(Guid.NewGuid(), nowUtc.Add(leaseDuration), nowUtc);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return candidates;
    }

    public async Task<IReadOnlyList<ExamPlaceWatchScan>> ListScansAsync(OrganizationId organizationId, ExamPlaceWatchSubscriptionId subscriptionId, int take, CancellationToken cancellationToken = default) =>
        await dbContext.ExamPlaceWatchScans.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.SubscriptionId == subscriptionId)
            .OrderByDescending(x => x.StartedAtUtc)
            .Take(Math.Clamp(take, 1, 200))
            .ToListAsync(cancellationToken);

    public Task<bool> HitExistsAsync(ExamPlaceWatchSubscriptionId subscriptionId, ExamPlaceId examPlaceId, CancellationToken cancellationToken = default) =>
        dbContext.ExamPlaceWatchHits.AsNoTracking().AnyAsync(x => x.SubscriptionId == subscriptionId && x.ExamPlaceId == examPlaceId, cancellationToken);

    public void Add(ExamPlaceWatchSubscription subscription) => dbContext.ExamPlaceWatchSubscriptions.Add(subscription);
    public void Add(ExamPlaceWatchScan scan) => dbContext.ExamPlaceWatchScans.Add(scan);
    public void Add(ExamPlaceWatchHit hit) => dbContext.ExamPlaceWatchHits.Add(hit);
}
