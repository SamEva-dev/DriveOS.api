using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Submissions;

public interface IExamRegistrationSubmissionRepository
{
    Task<ExamRegistrationSubmission?> GetByIdAsync(OrganizationId organizationId, ExamRegistrationSubmissionId id, CancellationToken cancellationToken = default);
    Task<ExamRegistrationSubmission?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamRegistrationSubmissionId id, CancellationToken cancellationToken = default);
    Task<ExamRegistrationSubmission?> FindByOperationIdAsync(OrganizationId organizationId, Guid operationId, CancellationToken cancellationToken = default);
    Task<ExamRegistrationSubmission?> FindByFileRevisionAsync(OrganizationId organizationId, ExamRegistrationId registrationId, Guid fileRevisionId, CancellationToken cancellationToken = default);
    Task<int> GetNextVersionAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamRegistrationSubmission>> ListByRegistrationAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default);
    void Add(ExamRegistrationSubmission submission);
}
