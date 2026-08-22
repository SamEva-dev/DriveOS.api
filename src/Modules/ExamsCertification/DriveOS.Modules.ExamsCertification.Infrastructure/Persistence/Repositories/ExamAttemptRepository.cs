using DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamAttemptRepository(ExamsCertificationDbContext db) : IExamAttemptRepository
{
    public Task<ExamAttempt?> GetByIdAsync(OrganizationId organizationId, ExamAttemptId attemptId, CancellationToken cancellationToken = default) =>
        db.ExamAttempts.AsNoTracking().Include(x => x.Timeline)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == attemptId, cancellationToken);

    public Task<ExamAttempt?> GetByRegistrationAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        db.ExamAttempts.AsNoTracking().Include(x => x.Timeline)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId, cancellationToken);

    public Task<ExamAttempt?> GetByRegistrationForUpdateAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        db.ExamAttempts.Include(x => x.Timeline)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId, cancellationToken);

    public async Task<int> GetNextAttemptNumberAsync(OrganizationId organizationId, PersonId studentId, string examType, string licenseCategory, CancellationToken cancellationToken = default)
    {
        int? max = await db.ExamAttempts.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId
                && x.StudentId == studentId
                && x.ExamType == examType
                && x.LicenseCategory == licenseCategory)
            .Select(x => (int?)x.AttemptNumber)
            .MaxAsync(cancellationToken);

        return (max ?? 0) + 1;
    }

    public void Add(ExamAttempt attempt) => db.ExamAttempts.Add(attempt);
}
