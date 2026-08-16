using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Repositories;

internal sealed class AssessmentSessionRepository(CrmDbContext context)
    : IAssessmentSessionRepository
{
    public void Add(AssessmentSession session) => context.AssessmentSessions.Add(session);

    public void AddRevision(AssessmentSessionRevision revision) =>
        context.AssessmentSessionRevisions.Add(revision);

    public Task<AssessmentSession?> GetByIdAsync(
        OrganizationId organizationId,
        AssessmentSessionId sessionId,
        CancellationToken cancellationToken = default
    ) =>
        context
            .AssessmentSessions.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == sessionId,
                cancellationToken
            );

    public Task<bool> ExistsForAppointmentAsync(
        OrganizationId organizationId,
        AssessmentAppointmentId appointmentId,
        CancellationToken cancellationToken = default
    ) =>
        context
            .AssessmentSessions.AsNoTracking()
            .AnyAsync(
                x => x.OrganizationId == organizationId && x.AppointmentId == appointmentId,
                cancellationToken
            );

    public Task<AssessmentSession?> GetByAppointmentAsync(
        OrganizationId organizationId,
        AssessmentAppointmentId appointmentId,
        CancellationToken cancellationToken = default
    ) =>
        context
            .AssessmentSessions.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.AppointmentId == appointmentId,
                cancellationToken
            );

    public Task<AssessmentSession?> GetByAppointmentForUpdateAsync(
        OrganizationId organizationId,
        AssessmentAppointmentId appointmentId,
        CancellationToken cancellationToken = default
    ) =>
        context.AssessmentSessions.SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.AppointmentId == appointmentId,
            cancellationToken
        );
}
