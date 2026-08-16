using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Assessments.ScheduleAssessment;

public sealed class ScheduleAssessmentCommandHandler(
    ILeadRepository leads,
    IAssessmentAppointmentRepository appointments,
    ICrmUnitOfWork unitOfWork
) : ICommandHandler<ScheduleAssessmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(ScheduleAssessmentCommand command, CancellationToken ct)
    {
        if (await leads.GetByIdAsync(command.OrganizationId, command.LeadId, ct) is null)
            return Result.Failure<Guid>(LeadErrors.NotFound);

        if (
            await appointments.HasSchedulingConflictAsync(
                command.OrganizationId,
                command.LeadId,
                command.StartsAtUtc,
                command.EndsAtUtc,
                command.EvaluatorUserId,
                command.VehicleId,
                command.RoomId,
                command.SimulatorId,
                cancellationToken: ct
            )
        )
            return Result.Failure<Guid>(AssessmentAppointmentErrors.SchedulingConflict);

        Result<AssessmentAppointment> result = AssessmentAppointment.Schedule(
            AssessmentAppointmentId.New(),
            command.OrganizationId,
            command.LeadId,
            command.BranchId,
            command.StartsAtUtc,
            command.EndsAtUtc,
            command.Type,
            command.DeliveryMode,
            command.LocationKind,
            command.LocationDetails,
            command.EvaluatorUserId,
            command.VehicleId,
            command.RoomId,
            command.SimulatorId,
            command.PriceAmount,
            command.PriceCurrency,
            command.Notes
        );

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        appointments.Add(result.Value);
        await unitOfWork.CommitAsync(ct);
        return Result.Success(result.Value.Id.Value);
    }
}
