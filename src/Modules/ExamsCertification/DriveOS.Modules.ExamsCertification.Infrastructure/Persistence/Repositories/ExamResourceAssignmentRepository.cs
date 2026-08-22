using DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamResourceAssignmentRepository(ExamsCertificationDbContext db) : IExamResourceAssignmentRepository
{
    public Task<ExamResourceAssignment?> GetByRegistrationAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        db.ExamResourceAssignments.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId, cancellationToken);

    public Task<ExamResourceAssignment?> GetByRegistrationForUpdateAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default) =>
        db.ExamResourceAssignments.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationId == registrationId, cancellationToken);

    public Task<ExamResourceAssignment?> GetByOperationIdAsync(OrganizationId organizationId, Guid operationId, CancellationToken cancellationToken = default) =>
        db.ExamResourceAssignments.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.OperationId == operationId, cancellationToken);

    public void Add(ExamResourceAssignment assignment) => db.ExamResourceAssignments.Add(assignment);
}
