using DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamOperationalPlanRepository(ExamsCertificationDbContext dbContext) : IExamOperationalPlanRepository
{
    public Task<ExamOperationalPlan?> GetByRegistrationAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        dbContext.ExamOperationalPlans.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId, cancellationToken);
    public Task<ExamOperationalPlan?> GetByRegistrationForUpdateAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        dbContext.ExamOperationalPlans.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId, cancellationToken);
    public void Add(ExamOperationalPlan plan) => dbContext.ExamOperationalPlans.Add(plan);
}
