using DriveOS.Modules.ExamsCertification.Domain.Registrations.Submissions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamRegistrationSubmissionRepository(ExamsCertificationDbContext dbContext)
    : IExamRegistrationSubmissionRepository
{
    public Task<ExamRegistrationSubmission?> GetByIdAsync(OrganizationId organizationId, ExamRegistrationSubmissionId id,
        CancellationToken cancellationToken = default) =>
        dbContext.ExamRegistrationSubmissions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    public Task<ExamRegistrationSubmission?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamRegistrationSubmissionId id,
        CancellationToken cancellationToken = default) =>
        dbContext.ExamRegistrationSubmissions
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    public Task<ExamRegistrationSubmission?> FindByOperationIdAsync(OrganizationId organizationId, Guid operationId,
        CancellationToken cancellationToken = default) =>
        dbContext.ExamRegistrationSubmissions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.OperationId == operationId, cancellationToken);

    public Task<ExamRegistrationSubmission?> FindByFileRevisionAsync(OrganizationId organizationId, ExamRegistrationId registrationId,
        Guid fileRevisionId, CancellationToken cancellationToken = default) =>
        dbContext.ExamRegistrationSubmissions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId && x.FileRevisionId == fileRevisionId, cancellationToken);

    public async Task<int> GetNextVersionAsync(OrganizationId organizationId, ExamRegistrationId registrationId,
        CancellationToken cancellationToken = default)
    {
        int? max = await dbContext.ExamRegistrationSubmissions.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId)
            .MaxAsync(x => (int?)x.SubmissionVersion, cancellationToken);
        return (max ?? 0) + 1;
    }

    public async Task<IReadOnlyList<ExamRegistrationSubmission>> ListByRegistrationAsync(OrganizationId organizationId,
        ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        await dbContext.ExamRegistrationSubmissions.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId)
            .OrderByDescending(x => x.SubmissionVersion)
            .ToListAsync(cancellationToken);

    public void Add(ExamRegistrationSubmission submission) => dbContext.ExamRegistrationSubmissions.Add(submission);
}
