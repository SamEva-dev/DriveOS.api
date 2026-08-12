using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Assessments.PerformAssessment;

internal sealed class StartAssessmentCommandHandler(IAssessmentAppointmentRepository appointments,
    IAssessmentSessionRepository sessions, ICrmUnitOfWork unitOfWork) : ICommandHandler<StartAssessmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(StartAssessmentCommand command, CancellationToken cancellationToken)
    {
        AssessmentAppointment? appointment = await appointments.GetByIdAsync(command.OrganizationId, command.AppointmentId, cancellationToken);
        if (appointment is null) return Result.Failure<Guid>(AssessmentSessionErrors.AppointmentNotFound);
        if (appointment.Status is not (AssessmentAppointmentStatus.Scheduled
            or AssessmentAppointmentStatus.Confirmed
            or AssessmentAppointmentStatus.Rescheduled))
            return Result.Failure<Guid>(AssessmentSessionErrors.AppointmentNotStartable);
        if (await sessions.ExistsForAppointmentAsync(command.OrganizationId, command.AppointmentId, cancellationToken))
            return Result.Failure<Guid>(AssessmentSessionErrors.AlreadyStarted);

        Result<AssessmentSession> created = AssessmentSession.Start(AssessmentSessionId.New(), command.OrganizationId,
            appointment.Id, appointment.LeadId, command.EvaluatorUserId, command.QuestionnaireCode,
            command.QuestionnaireVersion, command.QuestionnaireSnapshotJson, command.StartedAtUtc);
        if (created.IsFailure) return Result.Failure<Guid>(created.Error);
        sessions.Add(created.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(created.Value.Id.Value);
    }
}
