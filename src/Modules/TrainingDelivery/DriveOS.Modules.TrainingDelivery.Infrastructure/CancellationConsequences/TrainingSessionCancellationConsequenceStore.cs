using System.Text.Json;
using DriveOS.Modules.TrainingDelivery.Application.CancellationConsequences;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.CancellationConsequences;

internal sealed class TrainingSessionCancellationConsequenceStore(TrainingDeliveryDbContext db)
    : ITrainingSessionCancellationConsequenceStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task EnqueueAsync(
        SessionCancellation cancellation,
        CancellationToken cancellationToken = default)
    {
        SessionCancellationSnapshot snapshot = new(
            cancellation.OrganizationId,
            cancellation.Id,
            cancellation.TrainingSessionId,
            cancellation.SourceBookingId,
            cancellation.StudentOwnerOrganizationId,
            cancellation.PerformingOrganizationId,
            cancellation.StudentId,
            cancellation.InstructorId,
            cancellation.VehicleId,
            cancellation.ActualStartAtUtc,
            cancellation.CancelledAtUtc,
            cancellation.DeliveredDurationMinutes,
            cancellation.DistanceKilometers,
            cancellation.BillingDecision,
            cancellation.CreditDecision,
            cancellation.PartialCreditQuantity,
            cancellation.ProviderCompensationDecision,
            cancellation.TrainingCreditAccountId,
            cancellation.ReservedCreditQuantity,
            cancellation.CreditReservationReference,
            cancellation.PricingReference,
            cancellation.CancelledByUserId);

        var kinds = new List<TrainingSessionCancellationConsequenceKind>
        {
            TrainingSessionCancellationConsequenceKind.BillingReconciliation,
            TrainingSessionCancellationConsequenceKind.AnalyticsMetrics,
            TrainingSessionCancellationConsequenceKind.Communication
        };

        if (snapshot.TrainingCreditAccountId.HasValue
            && snapshot.ReservedCreditQuantity is > 0
            && !string.IsNullOrWhiteSpace(snapshot.CreditReservationReference))
        {
            kinds.Add(TrainingSessionCancellationConsequenceKind.TrainingCreditReconciliation);
        }

        if (snapshot.VehicleId.HasValue)
            kinds.Add(TrainingSessionCancellationConsequenceKind.VehicleUsage);

        if (snapshot.PerformingOrganizationId != snapshot.StudentOwnerOrganizationId)
            kinds.Add(TrainingSessionCancellationConsequenceKind.ProviderCompensation);

        string payload = JsonSerializer.Serialize(snapshot, JsonOptions);

        TrainingSessionCancellationConsequenceKind[] existing =
            await db.TrainingSessionCancellationConsequences
                .Where(x =>
                    x.OrganizationId == cancellation.OrganizationId
                    && x.CancellationId == cancellation.Id)
                .Select(x => x.Kind)
                .ToArrayAsync(cancellationToken);

        HashSet<TrainingSessionCancellationConsequenceKind> seen = existing.ToHashSet();

        foreach (TrainingSessionCancellationConsequenceKind kind in kinds.Where(x => !seen.Contains(x)))
        {
            db.TrainingSessionCancellationConsequences.Add(
                new TrainingSessionCancellationConsequenceMessage
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = cancellation.OrganizationId,
                    SessionId = cancellation.TrainingSessionId,
                    CancellationId = cancellation.Id,
                    Kind = kind,
                    Status = TrainingSessionConsequenceStatus.Pending,
                    PayloadJson = payload,
                    AttemptCount = 0,
                    CreatedAtUtc = cancellation.CreatedAtUtc,
                    NextAttemptAtUtc = cancellation.CreatedAtUtc
                });
        }
    }

    public async Task<IReadOnlyList<TrainingSessionCancellationConsequenceEnvelope>> ClaimDueAsync(
        int maxCount,
        DateTimeOffset now,
        TimeSpan processingLease,
        CancellationToken cancellationToken = default)
    {
        int take = Math.Clamp(maxCount, 1, 200);
        DateTimeOffset leaseExpiredBefore = now - processingLease;

        Guid[] candidateIds = await db.TrainingSessionCancellationConsequences
            .AsNoTracking()
            .Where(x =>
                ((x.Status == TrainingSessionConsequenceStatus.Pending
                    || x.Status == TrainingSessionConsequenceStatus.Failed
                    || x.Status == TrainingSessionConsequenceStatus.Deferred)
                    && (!x.NextAttemptAtUtc.HasValue || x.NextAttemptAtUtc <= now))
                || (x.Status == TrainingSessionConsequenceStatus.Processing
                    && x.LastAttemptAtUtc.HasValue
                    && x.LastAttemptAtUtc <= leaseExpiredBefore))
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => x.Id)
            .Take(Math.Min(400, take * 2))
            .ToArrayAsync(cancellationToken);

        var claimed = new List<TrainingSessionCancellationConsequenceEnvelope>(take);

        foreach (Guid id in candidateIds)
        {
            int affected = await db.TrainingSessionCancellationConsequences
                .Where(x =>
                    x.Id == id
                    && (
                        ((x.Status == TrainingSessionConsequenceStatus.Pending
                            || x.Status == TrainingSessionConsequenceStatus.Failed
                            || x.Status == TrainingSessionConsequenceStatus.Deferred)
                            && (!x.NextAttemptAtUtc.HasValue || x.NextAttemptAtUtc <= now))
                        || (x.Status == TrainingSessionConsequenceStatus.Processing
                            && x.LastAttemptAtUtc.HasValue
                            && x.LastAttemptAtUtc <= leaseExpiredBefore)))
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.Status, TrainingSessionConsequenceStatus.Processing)
                        .SetProperty(x => x.LastAttemptAtUtc, now)
                        .SetProperty(x => x.NextAttemptAtUtc, (DateTimeOffset?)null),
                    cancellationToken);

            if (affected != 1)
                continue;

            TrainingSessionCancellationConsequenceMessage? row =
                await db.TrainingSessionCancellationConsequences
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (row is not null)
                claimed.Add(ToEnvelope(row));

            if (claimed.Count >= take)
                break;
        }

        return claimed;
    }

    public async Task<IReadOnlyList<TrainingSessionCancellationConsequenceEnvelope>> GetBySessionAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        TrainingSessionCancellationConsequenceMessage[] rows =
            await db.TrainingSessionCancellationConsequences
                .AsNoTracking()
                .Where(x => x.OrganizationId == organizationId && x.SessionId == sessionId)
                .OrderBy(x => x.Kind)
                .ToArrayAsync(cancellationToken);

        return rows.Select(ToEnvelope).ToArray();
    }

    public Task MarkProcessedAsync(
        Guid id,
        DateTimeOffset now,
        CancellationToken cancellationToken = default) =>
        db.TrainingSessionCancellationConsequences
            .Where(x => x.Id == id && x.Status == TrainingSessionConsequenceStatus.Processing)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, TrainingSessionConsequenceStatus.Processed)
                    .SetProperty(x => x.ProcessedAtUtc, now)
                    .SetProperty(x => x.LastAttemptAtUtc, now)
                    .SetProperty(x => x.AttemptCount, x => x.AttemptCount + 1)
                    .SetProperty(x => x.NextAttemptAtUtc, (DateTimeOffset?)null)
                    .SetProperty(x => x.LastErrorCode, (string?)null)
                    .SetProperty(x => x.LastErrorDetail, (string?)null),
                cancellationToken);

    public Task MarkDeferredAsync(
        Guid id,
        string code,
        string? detail,
        DateTimeOffset attemptedAtUtc,
        DateTimeOffset nextAttemptAtUtc,
        CancellationToken cancellationToken = default) =>
        db.TrainingSessionCancellationConsequences
            .Where(x => x.Id == id && x.Status == TrainingSessionConsequenceStatus.Processing)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, TrainingSessionConsequenceStatus.Deferred)
                    .SetProperty(x => x.LastAttemptAtUtc, attemptedAtUtc)
                    .SetProperty(x => x.AttemptCount, x => x.AttemptCount + 1)
                    .SetProperty(x => x.NextAttemptAtUtc, nextAttemptAtUtc)
                    .SetProperty(x => x.LastErrorCode, code)
                    .SetProperty(x => x.LastErrorDetail, detail),
                cancellationToken);

    public Task MarkFailedAsync(
        Guid id,
        string code,
        string? detail,
        bool permanent,
        DateTimeOffset attemptedAtUtc,
        DateTimeOffset? nextAttemptAtUtc,
        CancellationToken cancellationToken = default) =>
        db.TrainingSessionCancellationConsequences
            .Where(x => x.Id == id && x.Status == TrainingSessionConsequenceStatus.Processing)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.Status,
                        permanent
                            ? TrainingSessionConsequenceStatus.DeadLetter
                            : TrainingSessionConsequenceStatus.Failed)
                    .SetProperty(x => x.LastAttemptAtUtc, attemptedAtUtc)
                    .SetProperty(x => x.AttemptCount, x => x.AttemptCount + 1)
                    .SetProperty(x => x.NextAttemptAtUtc, permanent ? null : nextAttemptAtUtc)
                    .SetProperty(x => x.LastErrorCode, code)
                    .SetProperty(x => x.LastErrorDetail, detail),
                cancellationToken);

    public Task RequeueAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default) =>
        db.TrainingSessionCancellationConsequences
            .Where(x =>
                x.OrganizationId == organizationId
                && x.SessionId == sessionId
                && x.Status != TrainingSessionConsequenceStatus.Processed)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, TrainingSessionConsequenceStatus.Pending)
                    .SetProperty(x => x.NextAttemptAtUtc, now)
                    .SetProperty(x => x.LastErrorCode, (string?)null)
                    .SetProperty(x => x.LastErrorDetail, (string?)null),
                cancellationToken);

    private static TrainingSessionCancellationConsequenceEnvelope ToEnvelope(
        TrainingSessionCancellationConsequenceMessage row) =>
        new(
            row.Id,
            row.OrganizationId,
            row.SessionId,
            row.CancellationId,
            row.Kind,
            row.Status,
            JsonSerializer.Deserialize<SessionCancellationSnapshot>(row.PayloadJson, JsonOptions)
                ?? throw new InvalidOperationException("Invalid cancellation consequence payload."),
            row.AttemptCount,
            row.CreatedAtUtc,
            row.LastAttemptAtUtc,
            row.NextAttemptAtUtc,
            row.ProcessedAtUtc,
            row.LastErrorCode,
            row.LastErrorDetail);
}
