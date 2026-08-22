using System.Text.Json;
using DriveOS.Modules.ExamsCertification.Application.Success;
using DriveOS.Modules.ExamsCertification.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Success;

internal sealed class ExamSuccessConsequenceStore(ExamsCertificationDbContext db) : IExamSuccessConsequenceStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly ExamSuccessConsequenceKind[] AllKinds =
    [
        ExamSuccessConsequenceKind.PedagogicalCompletion,
        ExamSuccessConsequenceKind.StudentJourneyTransition,
        ExamSuccessConsequenceKind.ContractCompletion,
        ExamSuccessConsequenceKind.FinancialClosureReview,
        ExamSuccessConsequenceKind.CertificationEligibility,
        ExamSuccessConsequenceKind.SchedulingFollowUpReview,
        ExamSuccessConsequenceKind.SuccessCommunication,
        ExamSuccessConsequenceKind.AnalyticsMetrics
    ];

    public async Task EnqueueAsync(ExamSuccessSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        string payload = JsonSerializer.Serialize(snapshot, JsonOptions);
        ExamSuccessConsequenceKind[] existing = await db.ExamSuccessConsequences
            .Where(x => x.OrganizationId == snapshot.OrganizationId && x.ResultId == snapshot.ResultId && x.ResultRevision == snapshot.ResultRevision)
            .Select(x => x.Kind).ToArrayAsync(cancellationToken);
        HashSet<ExamSuccessConsequenceKind> seen = existing.ToHashSet();

        foreach (ExamSuccessConsequenceKind kind in AllKinds.Where(x => !seen.Contains(x)))
        {
            db.ExamSuccessConsequences.Add(new ExamSuccessConsequenceMessage
            {
                Id = Guid.NewGuid(),
                OrganizationId = snapshot.OrganizationId,
                ResultId = snapshot.ResultId,
                ResultRevision = snapshot.ResultRevision,
                Kind = kind,
                Status = ExamSuccessConsequenceStatus.Pending,
                PayloadJson = payload,
                CreatedAtUtc = snapshot.ResultFinalizedAtUtc,
                NextAttemptAtUtc = snapshot.ResultFinalizedAtUtc
            });
        }
    }

    public async Task<IReadOnlyList<ExamSuccessConsequenceEnvelope>> ClaimDueAsync(int maxCount, DateTimeOffset now, TimeSpan processingLease, CancellationToken cancellationToken = default)
    {
        int take = Math.Clamp(maxCount, 1, 200);
        DateTimeOffset expiredLease = now - processingLease;
        Guid[] ids = await db.ExamSuccessConsequences.AsNoTracking()
            .Where(x => (((x.Status == ExamSuccessConsequenceStatus.Pending || x.Status == ExamSuccessConsequenceStatus.Failed || x.Status == ExamSuccessConsequenceStatus.Deferred)
                    && (!x.NextAttemptAtUtc.HasValue || x.NextAttemptAtUtc <= now))
                || (x.Status == ExamSuccessConsequenceStatus.Processing && x.LastAttemptAtUtc.HasValue && x.LastAttemptAtUtc <= expiredLease)))
            .OrderBy(x => x.CreatedAtUtc).Select(x => x.Id).Take(Math.Min(400, take * 2)).ToArrayAsync(cancellationToken);

        var claimed = new List<ExamSuccessConsequenceEnvelope>(take);
        foreach (Guid id in ids)
        {
            int affected = await db.ExamSuccessConsequences.Where(x => x.Id == id &&
                (((x.Status == ExamSuccessConsequenceStatus.Pending || x.Status == ExamSuccessConsequenceStatus.Failed || x.Status == ExamSuccessConsequenceStatus.Deferred)
                    && (!x.NextAttemptAtUtc.HasValue || x.NextAttemptAtUtc <= now))
                || (x.Status == ExamSuccessConsequenceStatus.Processing && x.LastAttemptAtUtc.HasValue && x.LastAttemptAtUtc <= expiredLease)))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, ExamSuccessConsequenceStatus.Processing)
                    .SetProperty(x => x.LastAttemptAtUtc, now).SetProperty(x => x.NextAttemptAtUtc, (DateTimeOffset?)null), cancellationToken);
            if (affected != 1) continue;
            ExamSuccessConsequenceMessage? row = await db.ExamSuccessConsequences.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (row is not null) claimed.Add(ToEnvelope(row));
            if (claimed.Count >= take) break;
        }
        return claimed;
    }

    public async Task<IReadOnlyList<ExamSuccessConsequenceEnvelope>> GetByResultAsync(OrganizationId organizationId, ExamResultId resultId, CancellationToken cancellationToken = default)
    {
        ExamSuccessConsequenceMessage[] rows = await db.ExamSuccessConsequences.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.ResultId == resultId)
            .OrderByDescending(x => x.ResultRevision).ThenBy(x => x.Kind).ToArrayAsync(cancellationToken);
        return rows.Select(ToEnvelope).ToArray();
    }

    public Task MarkProcessedAsync(Guid id, DateTimeOffset processedAtUtc, CancellationToken cancellationToken = default) =>
        db.ExamSuccessConsequences.Where(x => x.Id == id && x.Status == ExamSuccessConsequenceStatus.Processing)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, ExamSuccessConsequenceStatus.Processed)
                .SetProperty(x => x.ProcessedAtUtc, processedAtUtc).SetProperty(x => x.LastAttemptAtUtc, processedAtUtc)
                .SetProperty(x => x.AttemptCount, x => x.AttemptCount + 1).SetProperty(x => x.NextAttemptAtUtc, (DateTimeOffset?)null)
                .SetProperty(x => x.LastErrorCode, (string?)null).SetProperty(x => x.LastErrorDetail, (string?)null), cancellationToken);

    public Task MarkDeferredAsync(Guid id, string code, string? detail, DateTimeOffset attemptedAtUtc, DateTimeOffset nextAttemptAtUtc, CancellationToken cancellationToken = default) =>
        db.ExamSuccessConsequences.Where(x => x.Id == id && x.Status == ExamSuccessConsequenceStatus.Processing)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, ExamSuccessConsequenceStatus.Deferred)
                .SetProperty(x => x.LastAttemptAtUtc, attemptedAtUtc).SetProperty(x => x.AttemptCount, x => x.AttemptCount + 1)
                .SetProperty(x => x.NextAttemptAtUtc, nextAttemptAtUtc).SetProperty(x => x.LastErrorCode, code)
                .SetProperty(x => x.LastErrorDetail, detail), cancellationToken);

    public Task MarkFailedAsync(Guid id, string code, string? detail, bool permanent, DateTimeOffset attemptedAtUtc, DateTimeOffset? nextAttemptAtUtc, CancellationToken cancellationToken = default) =>
        db.ExamSuccessConsequences.Where(x => x.Id == id && x.Status == ExamSuccessConsequenceStatus.Processing)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, permanent ? ExamSuccessConsequenceStatus.DeadLetter : ExamSuccessConsequenceStatus.Failed)
                .SetProperty(x => x.LastAttemptAtUtc, attemptedAtUtc).SetProperty(x => x.AttemptCount, x => x.AttemptCount + 1)
                .SetProperty(x => x.NextAttemptAtUtc, permanent ? null : nextAttemptAtUtc).SetProperty(x => x.LastErrorCode, code)
                .SetProperty(x => x.LastErrorDetail, detail), cancellationToken);

    public Task SupersedeAsync(OrganizationId organizationId, ExamResultId resultId, int finalizedRevision, DateTimeOffset now, CancellationToken cancellationToken = default) =>
        db.ExamSuccessConsequences.Where(x => x.OrganizationId == organizationId && x.ResultId == resultId && x.ResultRevision == finalizedRevision && x.Status != ExamSuccessConsequenceStatus.Superseded)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, ExamSuccessConsequenceStatus.Superseded)
                .SetProperty(x => x.SupersededAtUtc, now).SetProperty(x => x.NextAttemptAtUtc, (DateTimeOffset?)null)
                .SetProperty(x => x.LastErrorCode, "exam-success.finalization-superseded")
                .SetProperty(x => x.LastErrorDetail, "The finalized passing result was corrected; downstream effects require reconciliation."), cancellationToken);

    public Task RequeueAsync(OrganizationId organizationId, ExamResultId resultId, DateTimeOffset now, CancellationToken cancellationToken = default) =>
        db.ExamSuccessConsequences.Where(x => x.OrganizationId == organizationId && x.ResultId == resultId &&
                x.Status != ExamSuccessConsequenceStatus.Processed && x.Status != ExamSuccessConsequenceStatus.Superseded)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, ExamSuccessConsequenceStatus.Pending)
                .SetProperty(x => x.NextAttemptAtUtc, now).SetProperty(x => x.LastErrorCode, (string?)null)
                .SetProperty(x => x.LastErrorDetail, (string?)null), cancellationToken);

    private static ExamSuccessConsequenceEnvelope ToEnvelope(ExamSuccessConsequenceMessage row) => new(row.Id, row.OrganizationId,
        row.ResultId, row.Kind, row.Status, JsonSerializer.Deserialize<ExamSuccessSnapshot>(row.PayloadJson, JsonOptions)
            ?? throw new InvalidOperationException("Invalid exam success consequence payload."), row.AttemptCount, row.CreatedAtUtc,
        row.LastAttemptAtUtc, row.NextAttemptAtUtc, row.ProcessedAtUtc, row.SupersededAtUtc, row.LastErrorCode, row.LastErrorDetail);
}
