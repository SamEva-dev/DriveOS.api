using DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamPreparationRepository(ExamsCertificationDbContext db) : IExamPreparationRepository
{
    public Task<ExamPreparation?> GetByRegistrationAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        db.ExamPreparations.AsNoTracking().Include(x => x.Checks)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId, cancellationToken);

    public Task<ExamPreparation?> GetByRegistrationForUpdateAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        db.ExamPreparations.Include(x => x.Checks)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId, cancellationToken);

    public void Add(ExamPreparation preparation) => db.ExamPreparations.Add(preparation);
}
