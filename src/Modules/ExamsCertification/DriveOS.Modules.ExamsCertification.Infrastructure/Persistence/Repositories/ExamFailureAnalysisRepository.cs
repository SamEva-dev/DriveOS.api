using DriveOS.Modules.ExamsCertification.Domain.Results.Failure;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamFailureAnalysisRepository(ExamsCertificationDbContext dbContext) : IExamFailureAnalysisRepository
{
    public Task<ExamFailureAnalysis?> GetLatestByResultAsync(OrganizationId organizationId, ExamResultId resultId, CancellationToken cancellationToken = default) =>
        dbContext.Set<ExamFailureAnalysis>().AsNoTracking().Include(x => x.Findings)
            .Where(x => x.OrganizationId == organizationId && x.ExamResultId == resultId)
            .OrderByDescending(x => x.ResultRevision).FirstOrDefaultAsync(cancellationToken);

    public Task<ExamFailureAnalysis?> GetByResultAsync(OrganizationId organizationId, ExamResultId resultId, int resultRevision, CancellationToken cancellationToken = default) =>
        dbContext.Set<ExamFailureAnalysis>().AsNoTracking().Include(x => x.Findings)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ExamResultId == resultId && x.ResultRevision == resultRevision, cancellationToken);

    public Task<ExamFailureAnalysis?> GetByResultForUpdateAsync(OrganizationId organizationId, ExamResultId resultId, int resultRevision, CancellationToken cancellationToken = default) =>
        dbContext.Set<ExamFailureAnalysis>().Include(x => x.Findings)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ExamResultId == resultId && x.ResultRevision == resultRevision, cancellationToken);

    public void Add(ExamFailureAnalysis analysis) => dbContext.Set<ExamFailureAnalysis>().Add(analysis);
}
