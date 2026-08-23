using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using DriveOS.Modules.RegulatoryIntegrations.Application.Dispatching;
using DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;
using DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Dispatching;

internal sealed class RegulatoryTrainingRecordSubmissionDispatchStore(RegulatoryIntegrationsDbContext db)
    : IRegulatoryTrainingRecordSubmissionDispatchStore
{
    public async Task<IReadOnlyList<RegulatoryTrainingRecordSubmissionDispatchEnvelope>> ClaimDueAsync(
        int batchSize,
        DateTimeOffset nowUtc,
        TimeSpan processingLease,
        CancellationToken cancellationToken = default)
    {
        int take = Math.Clamp(batchSize, 1, 500);
        DateTimeOffset now = nowUtc.ToUniversalTime();
        DateTimeOffset leaseUntil = now.Add(processingLease);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        List<RegulatoryTrainingRecordSubmission> due = await db.RegulatoryTrainingRecordSubmissions
            .FromSqlInterpolated($"""
                SELECT *
                FROM regulatory_integrations.training_record_submissions
                WHERE
                    ("Status" IN ({(int)RegulatoryTrainingRecordSubmissionStatus.Pending}, {(int)RegulatoryTrainingRecordSubmissionStatus.RetryPending})
                     AND ("NextAttemptAtUtc" IS NULL OR "NextAttemptAtUtc" <= {now}))
                    OR
                    ("Status" = {(int)RegulatoryTrainingRecordSubmissionStatus.Processing}
                     AND "NextAttemptAtUtc" IS NOT NULL
                     AND "NextAttemptAtUtc" <= {now})
                ORDER BY COALESCE("NextAttemptAtUtc", "CreatedAtUtc"), "CreatedAtUtc"
                LIMIT {take}
                FOR UPDATE SKIP LOCKED
                """)
            .AsTracking()
            .ToListAsync(cancellationToken);

        var envelopes = new List<RegulatoryTrainingRecordSubmissionDispatchEnvelope>(due.Count);

        foreach (RegulatoryTrainingRecordSubmission submission in due)
        {
            var start = submission.StartProcessing(now, leaseUntil);
            if (start.IsFailure)
                continue;

            envelopes.Add(new RegulatoryTrainingRecordSubmissionDispatchEnvelope(
                submission.Id.Value,
                submission.ProjectionId,
                submission.ProjectionSchemaVersion,
                submission.OrganizationId,
                submission.SessionId,
                submission.CountryCode,
                submission.ProviderCode,
                submission.PayloadJson,
                submission.PayloadHash,
                submission.AttemptCount));
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return envelopes;
    }

    public async Task ApplyResultAsync(
        Guid submissionId,
        RegulatoryTrainingRecordTransportResult result,
        DateTimeOffset attemptedAtUtc,
        DateTimeOffset? nextAttemptAtUtc,
        CancellationToken cancellationToken = default)
    {
        RegulatoryTrainingRecordSubmission submission = await GetRequiredAsync(submissionId, cancellationToken);

        var transition = result.Outcome switch
        {
            RegulatoryTrainingRecordTransportOutcome.Accepted =>
                submission.MarkAccepted(attemptedAtUtc, result.ExternalReference),

            RegulatoryTrainingRecordTransportOutcome.Rejected =>
                submission.MarkRejected(
                    attemptedAtUtc,
                    result.Code ?? "provider-rejected",
                    result.Detail,
                    result.ExternalReference),

            RegulatoryTrainingRecordTransportOutcome.Retry or RegulatoryTrainingRecordTransportOutcome.Unavailable =>
                submission.ScheduleRetry(
                    attemptedAtUtc,
                    nextAttemptAtUtc ?? attemptedAtUtc.ToUniversalTime().AddMinutes(5),
                    result.Code ?? (result.Outcome == RegulatoryTrainingRecordTransportOutcome.Unavailable
                        ? "provider-unavailable"
                        : "provider-retry"),
                    result.Detail),

            _ => submission.MarkFailed(attemptedAtUtc, "transport-outcome-unsupported", result.Outcome.ToString())
        };

        if (transition.IsFailure)
            throw new InvalidOperationException($"Cannot apply regulatory submission result: {transition.Error.Code}");

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid submissionId,
        string code,
        string? detail,
        DateTimeOffset attemptedAtUtc,
        CancellationToken cancellationToken = default)
    {
        RegulatoryTrainingRecordSubmission submission = await GetRequiredAsync(submissionId, cancellationToken);
        var transition = submission.MarkFailed(attemptedAtUtc, code, detail);
        if (transition.IsFailure)
            throw new InvalidOperationException($"Cannot fail regulatory submission: {transition.Error.Code}");
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<RegulatoryTrainingRecordSubmission> GetRequiredAsync(Guid submissionId, CancellationToken cancellationToken)
    {
        var id = new RegulatoryTrainingRecordSubmissionId(submissionId);
        return await db.RegulatoryTrainingRecordSubmissions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Regulatory submission '{submissionId}' was not found.");
    }
}
