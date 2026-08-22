using DriveOS.Modules.ExamsCertification.Domain.Registrations.File;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamRegistrationFileRepository(ExamsCertificationDbContext dbContext) : IExamRegistrationFileRepository
{
    public Task<ExamRegistrationFile?> GetByRegistrationAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        dbContext.ExamRegistrationFiles.AsNoTracking()
            .Include(x => x.Revisions).ThenInclude(x => x.Checklist)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId, cancellationToken);

    public Task<ExamRegistrationFile?> GetByRegistrationForUpdateAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        dbContext.ExamRegistrationFiles
            .Include(x => x.Revisions).ThenInclude(x => x.Checklist)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId, cancellationToken);

    public void Add(ExamRegistrationFile file) => dbContext.ExamRegistrationFiles.Add(file);
}
