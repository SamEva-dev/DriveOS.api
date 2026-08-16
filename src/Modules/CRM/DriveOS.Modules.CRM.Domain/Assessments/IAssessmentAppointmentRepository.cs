using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Assessments;

public interface IAssessmentAppointmentRepository
{
    void Add(AssessmentAppointment appointment);

    Task<AssessmentAppointment?> GetByIdAsync(
        OrganizationId organizationId,
        AssessmentAppointmentId appointmentId,
        CancellationToken cancellationToken = default
    );

    Task<AssessmentAppointment?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        AssessmentAppointmentId appointmentId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<AssessmentAppointment>> GetByLeadAsync(
        OrganizationId organizationId,
        LeadId leadId,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasSchedulingConflictAsync(
        OrganizationId organizationId,
        LeadId leadId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        UserId? evaluatorUserId,
        Guid? vehicleId,
        Guid? roomId,
        Guid? simulatorId,
        AssessmentAppointmentId? excludedAppointmentId = null,
        CancellationToken cancellationToken = default
    );
}
