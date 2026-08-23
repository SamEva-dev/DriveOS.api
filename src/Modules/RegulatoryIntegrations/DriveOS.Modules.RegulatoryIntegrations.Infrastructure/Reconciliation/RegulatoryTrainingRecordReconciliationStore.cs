using DriveOS.Modules.RegulatoryIntegrations.Application.Reconciliation;
using DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;
using DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Reconciliation;

internal sealed class RegulatoryTrainingRecordReconciliationStore(RegulatoryIntegrationsDbContext db)
    : IRegulatoryTrainingRecordReconciliationStore
{
    public async Task<IReadOnlyList<RegulatoryTrainingRecordReconciliationCandidate>> GetCandidatesAsync(
        int batchSize, CancellationToken cancellationToken = default)
    {
        int take = Math.Clamp(batchSize, 1, 500);
        return await db.RegulatoryTrainingRecordSubmissions
            .AsNoTracking()
            .Where(x => x.Status == RegulatoryTrainingRecordSubmissionStatus.WaitingForData
                     || x.Status == RegulatoryTrainingRecordSubmissionStatus.Rejected
                     || x.Status == RegulatoryTrainingRecordSubmissionStatus.Failed)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(take)
            .Select(x => new RegulatoryTrainingRecordReconciliationCandidate(x.Id.Value, x.PayloadJson))
            .ToListAsync(cancellationToken);
    }
}
