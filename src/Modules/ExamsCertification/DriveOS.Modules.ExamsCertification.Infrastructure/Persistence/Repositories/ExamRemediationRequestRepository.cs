using DriveOS.Modules.ExamsCertification.Domain.Remediation;
using DriveOS.Modules.ExamsCertification.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamRemediationRequestRepository(ExamsCertificationDbContext db) : IExamRemediationRequestRepository
{
    public Task<ExamRemediationRequest?> GetByIdAsync(OrganizationId organizationId, ExamRemediationRequestId id, CancellationToken ct = default) =>
        db.Set<ExamRemediationRequest>().AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, ct);
    public Task<ExamRemediationRequest?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamRemediationRequestId id, CancellationToken ct = default) =>
        db.Set<ExamRemediationRequest>().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, ct);
    public Task<ExamRemediationRequest?> GetByAnalysisAsync(OrganizationId organizationId, ExamFailureAnalysisId analysisId, CancellationToken ct = default) =>
        db.Set<ExamRemediationRequest>().AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.FailureAnalysisId == analysisId, ct);
    public Task<ExamRemediationRequest?> GetByResultRevisionForUpdateAsync(OrganizationId organizationId, ExamResultId resultId, int resultRevision, CancellationToken ct = default) =>
        db.Set<ExamRemediationRequest>().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ExamResultId == resultId && x.ResultRevision == resultRevision, ct);
    public async Task<IReadOnlyList<ExamRemediationRequest>> ListByStudentAsync(OrganizationId organizationId, PersonId studentId, CancellationToken ct = default) =>
        await db.Set<ExamRemediationRequest>().AsNoTracking().Where(x => x.OrganizationId == organizationId && x.StudentId == studentId)
            .OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);
    public void Add(ExamRemediationRequest request) => db.Set<ExamRemediationRequest>().Add(request);
}
