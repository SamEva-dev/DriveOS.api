using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamRegistrationRepository(ExamsCertificationDbContext dbContext) : IExamRegistrationRepository
{
    public Task<ExamRegistration?> GetByIdAsync(OrganizationId organizationId, ExamRegistrationId id, CancellationToken cancellationToken = default) =>
        dbContext.ExamRegistrations.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    public Task<ExamRegistration?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamRegistrationId id, CancellationToken cancellationToken = default) =>
        dbContext.ExamRegistrations.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    public Task<ExamRegistration?> FindByOperationIdAsync(OrganizationId organizationId, Guid operationId, CancellationToken cancellationToken = default) =>
        dbContext.ExamRegistrations.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.OperationId == operationId, cancellationToken);

    public Task<ExamRegistration?> FindActiveForStudentAsync(OrganizationId organizationId, PersonId studentId, string examType, string licenseCategory, CancellationToken cancellationToken = default) =>
        dbContext.ExamRegistrations.AsNoTracking().Where(x => x.OrganizationId == organizationId && x.StudentId == studentId && x.ExamType == examType && x.LicenseCategory == licenseCategory)
            .Where(x => x.Status == ExamRegistrationStatus.Draft || x.Status == ExamRegistrationStatus.PlaceAssigned || x.Status == ExamRegistrationStatus.PendingSubmission || x.Status == ExamRegistrationStatus.Submitted || x.Status == ExamRegistrationStatus.Confirmed)
            .OrderByDescending(x => x.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ExamRegistration>> ListForStudentAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default) =>
        await dbContext.ExamRegistrations.AsNoTracking().Where(x => x.OrganizationId == organizationId && x.StudentId == studentId)
            .OrderByDescending(x => x.ScheduledStartUtc).ToListAsync(cancellationToken);

    public void Add(ExamRegistration registration) => dbContext.ExamRegistrations.Add(registration);
}
