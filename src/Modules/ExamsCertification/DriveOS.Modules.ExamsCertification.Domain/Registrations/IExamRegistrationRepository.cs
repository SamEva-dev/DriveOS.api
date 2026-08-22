using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations;

public interface IExamRegistrationRepository
{
    Task<ExamRegistration?> GetByIdAsync(OrganizationId organizationId, ExamRegistrationId id, CancellationToken cancellationToken = default);
    Task<ExamRegistration?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamRegistrationId id, CancellationToken cancellationToken = default);
    Task<ExamRegistration?> FindByOperationIdAsync(OrganizationId organizationId, Guid operationId, CancellationToken cancellationToken = default);
    Task<ExamRegistration?> FindActiveForStudentAsync(OrganizationId organizationId, PersonId studentId, string examType, string licenseCategory, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamRegistration>> ListForStudentAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default);
    void Add(ExamRegistration registration);
}
