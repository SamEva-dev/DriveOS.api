using DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamConvocationRepository(ExamsCertificationDbContext dbContext) : IExamConvocationRepository
{
    public Task<ExamConvocation?> GetByRegistrationAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        dbContext.ExamConvocations.AsNoTracking().Include(x => x.Revisions)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId, cancellationToken);

    public Task<ExamConvocation?> GetByRegistrationForUpdateAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        dbContext.ExamConvocations.Include(x => x.Revisions)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId, cancellationToken);

    public void Add(ExamConvocation convocation) => dbContext.ExamConvocations.Add(convocation);
}
