using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Assessments.CancelAssessment;

public sealed class CancelAssessmentCommandHandler(
    IAssessmentAppointmentRepository repository,
    ICrmUnitOfWork unitOfWork
) : ICommandHandler<CancelAssessmentCommand>
{
    public async Task<Result> Handle(CancelAssessmentCommand command, CancellationToken ct)
    {
        AssessmentAppointment? appointment = await repository.GetByIdForUpdateAsync(
            command.OrganizationId,
            command.AppointmentId,
            ct
        );

        if (appointment is null)
            return Result.Failure(AssessmentAppointmentErrors.NotFound);

        Result result = appointment.Cancel(command.CancelledAtUtc);
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(ct);
        return Result.Success();
    }
}
