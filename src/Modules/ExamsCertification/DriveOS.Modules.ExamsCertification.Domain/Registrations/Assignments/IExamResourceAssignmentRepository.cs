using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments;

public interface IExamResourceAssignmentRepository
{
    Task<ExamResourceAssignment?> GetByRegistrationAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default);
    Task<ExamResourceAssignment?> GetByRegistrationForUpdateAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default);
    Task<ExamResourceAssignment?> GetByOperationIdAsync(OrganizationId organizationId, Guid operationId, CancellationToken cancellationToken = default);
    void Add(ExamResourceAssignment assignment);
}
