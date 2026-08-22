using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Readiness.Opinions;

public sealed class GetExamReadinessOpinionContextQueryHandler(IExamReadinessOpinionContextGateway gateway)
    : IQueryHandler<GetExamReadinessOpinionContextQuery, ExamReadinessOpinionContext>
{
    public Task<Result<ExamReadinessOpinionContext>> Handle(GetExamReadinessOpinionContextQuery query, CancellationToken cancellationToken) =>
        gateway.GetAsync(query.OrganizationId, query.StudentId, query.TrainingPathId, cancellationToken);
}

public sealed class SubmitExamReadinessOpinionCommandHandler(
    IExamReadinessOpinionRepository repository,
    IExamsCertificationUnitOfWork unitOfWork,
    IExamReadinessOpinionContextGateway contextGateway,
    IClock clock) : ICommandHandler<SubmitExamReadinessOpinionCommand, ExamReadinessOpinionId>
{
    public async Task<Result<ExamReadinessOpinionId>> Handle(SubmitExamReadinessOpinionCommand command, CancellationToken cancellationToken)
    {
        Result<ExamReadinessOpinionContext> contextResult = await contextGateway.GetAsync(
            command.OrganizationId,
            command.StudentId,
            command.TrainingPathId,
            cancellationToken);

        if (contextResult.IsFailure)
            return Result.Failure<ExamReadinessOpinionId>(contextResult.Error);

        ExamReadinessOpinionContext context = contextResult.Value;
        string fingerprint = ExamReadinessOpinion.CreateRequestFingerprint(
            command.Opinion,
            command.ObservedAutonomy,
            command.ReservationCodes,
            command.Reservations,
            command.Conditions,
            command.Comment,
            command.AuthorId);

        ExamReadinessOpinion? replay = await repository.GetByOperationIdAsync(
            command.OrganizationId,
            command.OperationId,
            cancellationToken);

        if (replay is not null)
        {
            return replay.IsReplayOf(fingerprint)
                ? Result.Success(replay.Id)
                : Result.Failure<ExamReadinessOpinionId>(ExamReadinessOpinionErrors.OperationConflict);
        }

        ExamReadinessOpinion? previous = await repository.GetLatestByAuthorAsync(
            command.OrganizationId,
            command.StudentId,
            command.TrainingPathId,
            command.AuthorId,
            cancellationToken);

        Result<ExamReadinessOpinion> creation = ExamReadinessOpinion.Submit(
            ExamReadinessOpinionId.New(),
            command.OrganizationId,
            command.StudentId,
            command.TrainingPathId,
            previous?.Id,
            (previous?.Version ?? 0) + 1,
            command.Opinion,
            command.ObservedAutonomy,
            command.ReservationCodes,
            command.Reservations,
            command.Conditions,
            command.Comment,
            context.ProgressPercent,
            context.RequiredCompetencies,
            context.EvaluatedRequiredCompetencies,
            context.HasCompletedPedagogicalReview,
            context.LatestPedagogicalDecision,
            command.OperationId,
            command.AuthorId,
            clock.UtcNow);

        if (creation.IsFailure)
            return Result.Failure<ExamReadinessOpinionId>(creation.Error);

        creation.Value.SetCreatedAudit(clock.UtcNow, command.AuthorId);
        repository.Add(creation.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(creation.Value.Id);
    }
}

public sealed class GetExamReadinessOpinionsQueryHandler(IExamReadinessOpinionRepository repository)
    : IQueryHandler<GetExamReadinessOpinionsQuery, IReadOnlyList<ExamReadinessOpinionResponse>>
{
    public async Task<Result<IReadOnlyList<ExamReadinessOpinionResponse>>> Handle(
        GetExamReadinessOpinionsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ExamReadinessOpinion> opinions = await repository.ListAsync(
            query.OrganizationId,
            query.StudentId,
            query.TrainingPathId,
            cancellationToken);

        IReadOnlyList<ExamReadinessOpinionResponse> response = opinions.Select(x => new ExamReadinessOpinionResponse(
            x.Id.Value,
            x.StudentId.Value,
            x.TrainingPathId.Value,
            x.PreviousOpinionId?.Value,
            x.Version,
            x.Opinion.ToString(),
            x.ObservedAutonomy.ToString(),
            x.ReservationCodes.Select(r => r.ToString()).ToArray(),
            x.Reservations,
            x.Conditions,
            x.Comment,
            x.ProgressPercent,
            x.RequiredCompetencies,
            x.EvaluatedRequiredCompetencies,
            x.HasCompletedPedagogicalReview,
            x.LatestPedagogicalDecision,
            x.AuthorId.Value,
            x.SubmittedAtUtc)).ToArray();

        return Result.Success(response);
    }
}
