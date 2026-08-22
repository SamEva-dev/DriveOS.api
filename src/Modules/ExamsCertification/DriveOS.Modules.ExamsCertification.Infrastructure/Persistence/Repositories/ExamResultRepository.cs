using DriveOS.Modules.ExamsCertification.Domain.Results;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamResultRepository(ExamsCertificationDbContext db) : IExamResultRepository
{
    public Task<ExamResult?> GetByIdAsync(OrganizationId o, ExamResultId id, CancellationToken ct = default) => db.ExamResults.AsNoTracking().Include(x=>x.Revisions).SingleOrDefaultAsync(x=>x.OrganizationId==o && x.Id==id,ct);
    public Task<ExamResult?> GetByIdForUpdateAsync(OrganizationId o, ExamResultId id, CancellationToken ct = default) => db.ExamResults.Include(x=>x.Revisions).SingleOrDefaultAsync(x=>x.OrganizationId==o && x.Id==id,ct);
    public Task<ExamResult?> GetByAttemptAsync(OrganizationId o, ExamAttemptId id, CancellationToken ct = default) => db.ExamResults.AsNoTracking().Include(x=>x.Revisions).SingleOrDefaultAsync(x=>x.OrganizationId==o && x.AttemptId==id,ct);
    public async Task<IReadOnlyList<ExamResult>> ListByStudentAsync(OrganizationId o, PersonId studentId, CancellationToken ct = default) =>
        await db.ExamResults.AsNoTracking().Include(x => x.Revisions)
            .Where(x => x.OrganizationId == o && x.StudentId == studentId)
            .OrderByDescending(x => x.ReceivedAtUtc)
            .ThenByDescending(x => x.AttemptNumber)
            .ToListAsync(ct);
    public Task<ExamResult?> FindByOperationIdAsync(OrganizationId o, Guid operationId, CancellationToken ct = default) => db.ExamResults.AsNoTracking().Include(x=>x.Revisions).FirstOrDefaultAsync(x=>x.OrganizationId==o && x.Revisions.Any(r=>r.OperationId==operationId),ct);
    public void Add(ExamResult result) => db.ExamResults.Add(result);
}
