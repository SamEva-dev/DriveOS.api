using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Assessments.RescheduleAssessment;

public sealed class RescheduleAssessmentCommandHandler(
    IAssessmentAppointmentRepository repository,
    ICrmUnitOfWork unitOfWork) : ICommandHandler<RescheduleAssessmentCommand>
{
    public async Task<Result> Handle(RescheduleAssessmentCommand command, CancellationToken ct)
    {
        AssessmentAppointment? appointment = await repository.GetByIdForUpdateAsync(
            command.OrganizationId,
            command.AppointmentId,
            ct);

        if (appointment is null)
            return Result.Failure(AssessmentAppointmentErrors.NotFound);

        if (await repository.HasSchedulingConflictAsync(
                command.OrganizationId,
                appointment.LeadId,
                command.StartsAtUtc,
                command.EndsAtUtc,
                appointment.EvaluatorUserId,
                appointment.VehicleId,
                appointment.RoomId,
                appointment.SimulatorId,
                appointment.Id,
                ct))
            return Result.Failure(AssessmentAppointmentErrors.SchedulingConflict);

        Result result = appointment.Reschedule(command.StartsAtUtc, command.EndsAtUtc);
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(ct);
        return Result.Success();
    }
}
