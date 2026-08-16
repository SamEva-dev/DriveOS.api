using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Repositories;

internal sealed class AssessmentAppointmentRepository(CrmDbContext context)
    : IAssessmentAppointmentRepository
{
    public void Add(AssessmentAppointment appointment) =>
        context.AssessmentAppointments.Add(appointment);

    public Task<AssessmentAppointment?> GetByIdAsync(
        OrganizationId organizationId,
        AssessmentAppointmentId appointmentId,
        CancellationToken cancellationToken = default
    ) =>
        context
            .AssessmentAppointments.AsNoTracking()
            .SingleOrDefaultAsync(
                appointment =>
                    appointment.OrganizationId == organizationId && appointment.Id == appointmentId,
                cancellationToken
            );

    public Task<AssessmentAppointment?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        AssessmentAppointmentId appointmentId,
        CancellationToken cancellationToken = default
    ) =>
        context.AssessmentAppointments.SingleOrDefaultAsync(
            appointment =>
                appointment.OrganizationId == organizationId && appointment.Id == appointmentId,
            cancellationToken
        );

    public async Task<IReadOnlyList<AssessmentAppointment>> GetByLeadAsync(
        OrganizationId organizationId,
        LeadId leadId,
        CancellationToken cancellationToken = default
    ) =>
        await context
            .AssessmentAppointments.AsNoTracking()
            .Where(appointment =>
                appointment.OrganizationId == organizationId && appointment.LeadId == leadId
            )
            .OrderByDescending(appointment => appointment.StartsAtUtc)
            .ToListAsync(cancellationToken);

    public Task<bool> HasSchedulingConflictAsync(
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
    )
    {
        IQueryable<AssessmentAppointment> query = context
            .AssessmentAppointments.AsNoTracking()
            .Where(appointment =>
                appointment.OrganizationId == organizationId
                && appointment.Status != AssessmentAppointmentStatus.Completed
                && appointment.Status != AssessmentAppointmentStatus.Cancelled
                && appointment.Status != AssessmentAppointmentStatus.NoShow
                && appointment.StartsAtUtc < endsAtUtc
                && startsAtUtc < appointment.EndsAtUtc
            );

        if (excludedAppointmentId.HasValue)
        {
            AssessmentAppointmentId appointmentId = excludedAppointmentId.Value;
            query = query.Where(appointment => appointment.Id != appointmentId);
        }

        query = query.Where(appointment => appointment.LeadId == leadId);

        if (evaluatorUserId.HasValue)
        {
            UserId evaluatorId = evaluatorUserId.Value;
            query = query.Concat(
                context
                    .AssessmentAppointments.AsNoTracking()
                    .Where(appointment =>
                        appointment.OrganizationId == organizationId
                        && appointment.Status != AssessmentAppointmentStatus.Completed
                        && appointment.Status != AssessmentAppointmentStatus.Cancelled
                        && appointment.Status != AssessmentAppointmentStatus.NoShow
                        && appointment.StartsAtUtc < endsAtUtc
                        && startsAtUtc < appointment.EndsAtUtc
                        && appointment.EvaluatorUserId == evaluatorId
                        && (
                            !excludedAppointmentId.HasValue
                            || appointment.Id != excludedAppointmentId.Value
                        )
                    )
            );
        }

        if (vehicleId.HasValue)
            query = AddResourceConflicts(
                query,
                organizationId,
                startsAtUtc,
                endsAtUtc,
                excludedAppointmentId,
                appointment => appointment.VehicleId == vehicleId
            );
        if (roomId.HasValue)
            query = AddResourceConflicts(
                query,
                organizationId,
                startsAtUtc,
                endsAtUtc,
                excludedAppointmentId,
                appointment => appointment.RoomId == roomId
            );
        if (simulatorId.HasValue)
            query = AddResourceConflicts(
                query,
                organizationId,
                startsAtUtc,
                endsAtUtc,
                excludedAppointmentId,
                appointment => appointment.SimulatorId == simulatorId
            );

        return query.AnyAsync(cancellationToken);
    }

    private IQueryable<AssessmentAppointment> AddResourceConflicts(
        IQueryable<AssessmentAppointment> query,
        OrganizationId organizationId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        AssessmentAppointmentId? excludedAppointmentId,
        System.Linq.Expressions.Expression<Func<AssessmentAppointment, bool>> resourcePredicate
    ) =>
        query.Concat(
            context
                .AssessmentAppointments.AsNoTracking()
                .Where(appointment =>
                    appointment.OrganizationId == organizationId
                    && appointment.Status != AssessmentAppointmentStatus.Completed
                    && appointment.Status != AssessmentAppointmentStatus.Cancelled
                    && appointment.Status != AssessmentAppointmentStatus.NoShow
                    && appointment.StartsAtUtc < endsAtUtc
                    && startsAtUtc < appointment.EndsAtUtc
                    && (
                        !excludedAppointmentId.HasValue
                        || appointment.Id != excludedAppointmentId.Value
                    )
                )
                .Where(resourcePredicate)
        );
}
