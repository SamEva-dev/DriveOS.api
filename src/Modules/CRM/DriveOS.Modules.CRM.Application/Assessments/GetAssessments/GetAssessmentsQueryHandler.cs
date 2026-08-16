using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Assessments.GetAssessments;

public sealed class GetLeadAssessmentsQueryHandler(IAssessmentAppointmentRepository repository)
    : IQueryHandler<GetLeadAssessmentsQuery, IReadOnlyList<AssessmentAppointmentResponse>>
{
    public async Task<Result<IReadOnlyList<AssessmentAppointmentResponse>>> Handle(
        GetLeadAssessmentsQuery query,
        CancellationToken ct
    )
    {
        IReadOnlyList<AssessmentAppointment> appointments = await repository.GetByLeadAsync(
            query.OrganizationId,
            query.LeadId,
            ct
        );

        return Result.Success<IReadOnlyList<AssessmentAppointmentResponse>>(
            appointments.Select(Map).ToArray()
        );
    }

    internal static AssessmentAppointmentResponse Map(AssessmentAppointment appointment) =>
        new(
            appointment.Id.Value,
            appointment.LeadId.Value,
            appointment.BranchId?.Value,
            appointment.StartsAtUtc,
            appointment.EndsAtUtc,
            appointment.Type.ToString(),
            appointment.DeliveryMode.ToString(),
            appointment.LocationKind.ToString(),
            appointment.LocationDetails,
            appointment.EvaluatorUserId?.Value,
            appointment.VehicleId,
            appointment.RoomId,
            appointment.SimulatorId,
            appointment.PriceAmount,
            appointment.PriceCurrency,
            appointment.Notes,
            appointment.Status.ToString(),
            appointment.ClosedAtUtc,
            appointment.CreatedAtUtc
        );
}

public sealed class GetAssessmentQueryHandler(IAssessmentAppointmentRepository repository)
    : IQueryHandler<GetAssessmentQuery, AssessmentAppointmentResponse>
{
    public async Task<Result<AssessmentAppointmentResponse>> Handle(
        GetAssessmentQuery query,
        CancellationToken ct
    )
    {
        AssessmentAppointment? appointment = await repository.GetByIdAsync(
            query.OrganizationId,
            query.AppointmentId,
            ct
        );

        return appointment is null
            ? Result.Failure<AssessmentAppointmentResponse>(AssessmentAppointmentErrors.NotFound)
            : Result.Success(GetLeadAssessmentsQueryHandler.Map(appointment));
    }
}
