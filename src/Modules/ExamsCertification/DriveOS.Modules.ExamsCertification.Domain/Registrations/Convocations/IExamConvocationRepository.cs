using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;

public interface IExamConvocationRepository
{
    Task<ExamConvocation?> GetByRegistrationAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default);
    Task<ExamConvocation?> GetByRegistrationForUpdateAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default);
    void Add(ExamConvocation convocation);
}
