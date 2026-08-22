using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;

public interface IExamAttemptRepository
{
    Task<ExamAttempt?> GetByIdAsync(OrganizationId organizationId, ExamAttemptId attemptId, CancellationToken cancellationToken = default);
    Task<ExamAttempt?> GetByRegistrationAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default);
    Task<ExamAttempt?> GetByRegistrationForUpdateAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default);
    Task<int> GetNextAttemptNumberAsync(OrganizationId organizationId, PersonId studentId, string examType, string licenseCategory, CancellationToken cancellationToken = default);
    void Add(ExamAttempt attempt);
}
