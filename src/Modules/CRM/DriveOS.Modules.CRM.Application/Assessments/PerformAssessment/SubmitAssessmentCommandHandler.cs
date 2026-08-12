using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Assessments.PerformAssessment;

internal sealed class SubmitAssessmentCommandHandler(IAssessmentSessionRepository sessions,
    IAssessmentAppointmentRepository appointments, ICrmUnitOfWork unitOfWork) : ICommandHandler<SubmitAssessmentCommand>
{
    public async Task<Result> Handle(SubmitAssessmentCommand command, CancellationToken cancellationToken)
    {
        AssessmentSession? session = await sessions.GetByAppointmentForUpdateAsync(command.OrganizationId, command.AppointmentId, cancellationToken);
        if (session is null) return Result.Failure(AssessmentSessionErrors.NotFound);
        AssessmentAppointment? appointment = await appointments.GetByIdForUpdateAsync(command.OrganizationId, command.AppointmentId, cancellationToken);
        if (appointment is null) return Result.Failure(AssessmentSessionErrors.AppointmentNotFound);
        Result submitted = session.Submit(command.SubmittedByUserId, command.SubmittedAtUtc);
        if (submitted.IsFailure) return submitted;
        Result completed = appointment.Complete(command.SubmittedAtUtc);
        if (completed.IsFailure) return completed;
        sessions.AddRevision(AssessmentSessionRevision.Capture(session, command.SubmittedByUserId, command.SubmittedAtUtc));
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
