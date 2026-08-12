using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Assessments;

public interface IAssessmentSessionRepository
{
    void Add(AssessmentSession session);
    void AddRevision(AssessmentSessionRevision revision);
    Task<AssessmentSession?> GetByIdAsync(OrganizationId organizationId, AssessmentSessionId sessionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForAppointmentAsync(OrganizationId organizationId, AssessmentAppointmentId appointmentId, CancellationToken cancellationToken = default);
    Task<AssessmentSession?> GetByAppointmentAsync(OrganizationId organizationId, AssessmentAppointmentId appointmentId, CancellationToken cancellationToken = default);
    Task<AssessmentSession?> GetByAppointmentForUpdateAsync(OrganizationId organizationId, AssessmentAppointmentId appointmentId, CancellationToken cancellationToken = default);
}
