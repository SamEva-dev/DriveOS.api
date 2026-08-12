using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Assessments.PerformAssessment;

internal sealed class SaveAssessmentDraftCommandHandler(IAssessmentSessionRepository sessions, ICrmUnitOfWork unitOfWork)
    : ICommandHandler<SaveAssessmentDraftCommand>
{
    public async Task<Result> Handle(SaveAssessmentDraftCommand command, CancellationToken cancellationToken)
    {
        AssessmentSession? session = await sessions.GetByAppointmentForUpdateAsync(command.OrganizationId, command.AppointmentId, cancellationToken);
        if (session is null) return Result.Failure(AssessmentSessionErrors.NotFound);
        Result result = session.SaveDraft(command.AnswersJson, command.FactualObservations,
            command.PedagogicalInterpretation, command.Recommendation, command.InternalNotes,
            command.ProspectComment, command.DraftCompleted, command.SavedAtUtc);
        if (result.IsFailure) return result;
        sessions.AddRevision(AssessmentSessionRevision.Capture(session, command.SavedByUserId, command.SavedAtUtc));
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
