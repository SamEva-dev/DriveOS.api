using DriveOS.Modules.ExamsCertification.Domain.Results.Success;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;
internal sealed class ExamSuccessProcessRepository(ExamsCertificationDbContext dbContext) : IExamSuccessProcessRepository
{
    public Task<ExamSuccessProcess?> GetLatestByResultAsync(OrganizationId organizationId, ExamResultId resultId, CancellationToken cancellationToken = default) =>
        dbContext.Set<ExamSuccessProcess>().AsNoTracking().Include(x => x.Actions).Where(x => x.OrganizationId == organizationId && x.ExamResultId == resultId).OrderByDescending(x => x.ResultRevision).FirstOrDefaultAsync(cancellationToken);
    public Task<ExamSuccessProcess?> GetByResultAsync(OrganizationId organizationId, ExamResultId resultId, int resultRevision, CancellationToken cancellationToken = default) =>
        dbContext.Set<ExamSuccessProcess>().AsNoTracking().Include(x => x.Actions).SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ExamResultId == resultId && x.ResultRevision == resultRevision, cancellationToken);
    public Task<ExamSuccessProcess?> GetByResultForUpdateAsync(OrganizationId organizationId, ExamResultId resultId, int resultRevision, CancellationToken cancellationToken = default) =>
        dbContext.Set<ExamSuccessProcess>().Include(x => x.Actions).SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ExamResultId == resultId && x.ResultRevision == resultRevision, cancellationToken);
    public void Add(ExamSuccessProcess process) => dbContext.Set<ExamSuccessProcess>().Add(process);
}
