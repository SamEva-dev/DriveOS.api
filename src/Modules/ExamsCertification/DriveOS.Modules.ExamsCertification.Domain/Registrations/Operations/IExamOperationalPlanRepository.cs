using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;

public interface IExamOperationalPlanRepository
{
    Task<ExamOperationalPlan?> GetByRegistrationAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default);
    Task<ExamOperationalPlan?> GetByRegistrationForUpdateAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default);
    void Add(ExamOperationalPlan plan);
}
