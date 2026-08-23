using DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Persistence.Repositories;
internal sealed class RegulatoryTrainingRecordSubmissionRepository(RegulatoryIntegrationsDbContext db) : IRegulatoryTrainingRecordSubmissionRepository
{
    public Task<RegulatoryTrainingRecordSubmission?> GetLatestAsync(Guid projectionId, string providerCode, CancellationToken cancellationToken = default) =>
        db.RegulatoryTrainingRecordSubmissions
            .Where(x => x.ProjectionId == projectionId && x.ProviderCode == providerCode)
            .OrderByDescending(x => x.Revision)
            .FirstOrDefaultAsync(cancellationToken);

    public void Add(RegulatoryTrainingRecordSubmission submission) => db.RegulatoryTrainingRecordSubmissions.Add(submission);
}
