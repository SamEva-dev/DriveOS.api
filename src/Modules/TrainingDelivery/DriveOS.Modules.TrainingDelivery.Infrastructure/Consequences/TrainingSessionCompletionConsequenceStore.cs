using System.Text.Json;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Consequences;

internal sealed class TrainingSessionCompletionConsequenceStore(TrainingDeliveryDbContext db)
    : ITrainingSessionCompletionConsequenceStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task EnqueueAsync(
        TrainingSessionCompletionSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        var kinds = new List<TrainingSessionConsequenceKind>
        {
            TrainingSessionConsequenceKind.BillingServiceRecognition,
            TrainingSessionConsequenceKind.AnalyticsMetrics,
            TrainingSessionConsequenceKind.SessionSummaryCommunication,
            TrainingSessionConsequenceKind.RegulatoryTrainingRecordSubmission
        };

        if (snapshot.TrainingCreditAccountId.HasValue
            && snapshot.CreditQuantity is > 0
            && !string.IsNullOrWhiteSpace(snapshot.CreditReservationReference))
        {
            kinds.Add(TrainingSessionConsequenceKind.TrainingCreditConsumption);
        }

        if (snapshot.VehicleId.HasValue)
            kinds.Add(TrainingSessionConsequenceKind.VehicleUsage);

        if (snapshot.PerformingOrganizationId != snapshot.StudentOwnerOrganizationId)
            kinds.Add(TrainingSessionConsequenceKind.ProfessionalServiceEntry);

        string payload = JsonSerializer.Serialize(snapshot, JsonOptions);

        TrainingSessionConsequenceKind[] existing = await db.TrainingSessionCompletionConsequences
            .Where(x => x.OrganizationId == snapshot.OrganizationId && x.SessionId == snapshot.SessionId)
            .Select(x => x.Kind)
            .ToArrayAsync(cancellationToken);

        HashSet<TrainingSessionConsequenceKind> seen = existing.ToHashSet();

        foreach (TrainingSessionConsequenceKind kind in kinds.Where(x => !seen.Contains(x)))
        {
            db.TrainingSessionCompletionConsequences.Add(new TrainingSessionCompletionConsequenceMessage
            {
                Id = Guid.NewGuid(),
                OrganizationId = snapshot.OrganizationId,
                SessionId = snapshot.SessionId,
                Kind = kind,
                Status = TrainingSessionConsequenceStatus.Pending,
                PayloadJson = payload,
                AttemptCount = 0,
                CreatedAtUtc = snapshot.CompletedAtUtc,
                NextAttemptAtUtc = snapshot.CompletedAtUtc
            });
        }
    }

    public async Task<IReadOnlyList<TrainingSessionConsequenceEnvelope>> ClaimDueAsync(
        int maxCount,
        DateTimeOffset now,
        TimeSpan processingLease,
        CancellationToken cancellationToken = default)
    {
        int take = Math.Clamp(maxCount, 1, 200);
        DateTimeOffset leaseExpiredBefore = now - processingLease;

        Guid[] candidateIds = await db.TrainingSessionCompletionConsequences
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

        var claimed = new List<TrainingSessionConsequenceEnvelope>(take);

        foreach (Guid id in candidateIds)
        {
            int affected = await db.TrainingSessionCompletionConsequences
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

            TrainingSessionCompletionConsequenceMessage? row =
                await db.TrainingSessionCompletionConsequences
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (row is not null)
                claimed.Add(ToEnvelope(row));

            if (claimed.Count >= take)
                break;
        }

        return claimed;
    }

    public async Task<IReadOnlyList<TrainingSessionConsequenceEnvelope>> GetBySessionAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        TrainingSessionCompletionConsequenceMessage[] rows =
            await db.TrainingSessionCompletionConsequences
                .AsNoTracking()
                .Where(x => x.OrganizationId == organizationId && x.SessionId == sessionId)
                .OrderBy(x => x.Kind)
                .ToArrayAsync(cancellationToken);

        return rows.Select(ToEnvelope).ToArray();
    }

    public Task MarkProcessedAsync(
        Guid id,
        DateTimeOffset processedAtUtc,
        CancellationToken cancellationToken = default) =>
        db.TrainingSessionCompletionConsequences
            .Where(x => x.Id == id && x.Status == TrainingSessionConsequenceStatus.Processing)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, TrainingSessionConsequenceStatus.Processed)
                    .SetProperty(x => x.ProcessedAtUtc, processedAtUtc)
                    .SetProperty(x => x.LastAttemptAtUtc, processedAtUtc)
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
        db.TrainingSessionCompletionConsequences
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
        db.TrainingSessionCompletionConsequences
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
        db.TrainingSessionCompletionConsequences
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

    private static TrainingSessionConsequenceEnvelope ToEnvelope(
        TrainingSessionCompletionConsequenceMessage row) =>
        new(
            row.Id,
            row.OrganizationId,
            row.SessionId,
            row.Kind,
            row.Status,
            JsonSerializer.Deserialize<TrainingSessionCompletionSnapshot>(row.PayloadJson, JsonOptions)
                ?? throw new InvalidOperationException("Invalid completion consequence payload."),
            row.AttemptCount,
            row.CreatedAtUtc,
            row.LastAttemptAtUtc,
            row.NextAttemptAtUtc,
            row.ProcessedAtUtc,
            row.LastErrorCode,
            row.LastErrorDetail);
}
