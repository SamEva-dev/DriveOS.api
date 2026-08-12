using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Assessments.AssessmentResult;

public sealed record SaveAssessmentResultCommand(
    OrganizationId OrganizationId,
    AssessmentAppointmentId AppointmentId,
    UserId SavedByUserId,
    int ExpectedRevision,
    string ResultJson,
    AssessmentResultConfidence Confidence,
    string? AiSuggestionJson,
    DateTimeOffset SavedAtUtc) : ICommand;

public sealed record RequestAssessmentResultCorrectionCommand(
    OrganizationId OrganizationId,
    AssessmentAppointmentId AppointmentId,
    UserId RequestedByUserId,
    int ExpectedRevision,
    string Reason,
    DateTimeOffset RequestedAtUtc) : ICommand;

public sealed record ValidateAssessmentResultCommand(
    OrganizationId OrganizationId,
    AssessmentAppointmentId AppointmentId,
    UserId ValidatedByUserId,
    int ExpectedRevision,
    DateTimeOffset ValidatedAtUtc) : ICommand;

public sealed record ShareAssessmentResultCommand(
    OrganizationId OrganizationId,
    AssessmentAppointmentId AppointmentId,
    UserId SharedByUserId,
    int ExpectedRevision,
    DateTimeOffset SharedAtUtc) : ICommand;

internal abstract class AssessmentResultCommandHandlerBase(
    IAssessmentSessionRepository sessions,
    ICrmUnitOfWork unitOfWork)
{
    protected async Task<Result> ExecuteAsync(
        OrganizationId organizationId,
        AssessmentAppointmentId appointmentId,
        UserId actorUserId,
        int expectedRevision,
        DateTimeOffset occurredAtUtc,
        Func<AssessmentSession, Result> operation,
        CancellationToken cancellationToken)
    {
        AssessmentSession? session = await sessions.GetByAppointmentForUpdateAsync(
            organizationId, appointmentId, cancellationToken);
        if (session is null)
            return Result.Failure(AssessmentSessionErrors.NotFound);
        if (session.Revision != expectedRevision)
            return Result.Failure(AssessmentSessionErrors.RevisionConflict);

        Result result = operation(session);
        if (result.IsFailure)
            return result;

        sessions.AddRevision(AssessmentSessionRevision.Capture(
            session, actorUserId, occurredAtUtc));
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class SaveAssessmentResultCommandHandler(
    IAssessmentSessionRepository sessions,
    ICrmUnitOfWork unitOfWork)
    : AssessmentResultCommandHandlerBase(sessions, unitOfWork),
      ICommandHandler<SaveAssessmentResultCommand>
{
    public Task<Result> Handle(
        SaveAssessmentResultCommand command,
        CancellationToken cancellationToken) =>
        ExecuteAsync(command.OrganizationId, command.AppointmentId,
            command.SavedByUserId, command.ExpectedRevision, command.SavedAtUtc,
            session => session.SaveResult(command.ResultJson, command.Confidence,
                command.AiSuggestionJson, command.SavedByUserId, command.SavedAtUtc),
            cancellationToken);
}

internal sealed class RequestAssessmentResultCorrectionCommandHandler(
    IAssessmentSessionRepository sessions,
    ICrmUnitOfWork unitOfWork)
    : AssessmentResultCommandHandlerBase(sessions, unitOfWork),
      ICommandHandler<RequestAssessmentResultCorrectionCommand>
{
    public Task<Result> Handle(
        RequestAssessmentResultCorrectionCommand command,
        CancellationToken cancellationToken) =>
        ExecuteAsync(command.OrganizationId, command.AppointmentId,
            command.RequestedByUserId, command.ExpectedRevision, command.RequestedAtUtc,
            session => session.RequestResultCorrection(command.Reason,
                command.RequestedByUserId, command.RequestedAtUtc),
            cancellationToken);
}

internal sealed class ValidateAssessmentResultCommandHandler(
    IAssessmentSessionRepository sessions,
    ICrmUnitOfWork unitOfWork)
    : AssessmentResultCommandHandlerBase(sessions, unitOfWork),
      ICommandHandler<ValidateAssessmentResultCommand>
{
    public Task<Result> Handle(
        ValidateAssessmentResultCommand command,
        CancellationToken cancellationToken) =>
        ExecuteAsync(command.OrganizationId, command.AppointmentId,
            command.ValidatedByUserId, command.ExpectedRevision, command.ValidatedAtUtc,
            session => session.ValidateResult(command.ValidatedByUserId,
                command.ValidatedAtUtc),
            cancellationToken);
}

internal sealed class ShareAssessmentResultCommandHandler(
    IAssessmentSessionRepository sessions,
    ICrmUnitOfWork unitOfWork)
    : AssessmentResultCommandHandlerBase(sessions, unitOfWork),
      ICommandHandler<ShareAssessmentResultCommand>
{
    public Task<Result> Handle(
        ShareAssessmentResultCommand command,
        CancellationToken cancellationToken) =>
        ExecuteAsync(command.OrganizationId, command.AppointmentId,
            command.SharedByUserId, command.ExpectedRevision, command.SharedAtUtc,
            session => session.MarkResultShared(command.SharedByUserId,
                command.SharedAtUtc),
            cancellationToken);
}
