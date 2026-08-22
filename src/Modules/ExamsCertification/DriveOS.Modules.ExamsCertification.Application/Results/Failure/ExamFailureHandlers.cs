using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Results.Failure;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Results.Failure;

public sealed class GetExamFailureAnalysisQueryHandler(IExamFailureAnalysisRepository repository)
    : IQueryHandler<GetExamFailureAnalysisQuery, ExamFailureAnalysisResponse>
{
    public async Task<Result<ExamFailureAnalysisResponse>> Handle(GetExamFailureAnalysisQuery q, CancellationToken ct)
    {
        ExamFailureAnalysis? x = await repository.GetLatestByResultAsync(q.OrganizationId, q.ResultId, ct);
        return x is null ? Result.Failure<ExamFailureAnalysisResponse>(ExamFailureAnalysisErrors.NotFound) : Result.Success(Map(x));
    }

    internal static ExamFailureAnalysisResponse Map(ExamFailureAnalysis x) => new(x.Id.Value, x.ExamResultId.Value, x.ResultRevision,
        x.AttemptId.Value, x.RegistrationId.Value, x.StudentId.Value, x.AttemptNumber, x.Status.ToString(), x.InstructorAnalysis,
        x.StudentFeedback, x.Summary, x.Recommendation, x.CompletedAtUtc, x.CompletedByUserId?.Value, x.SupersededAtUtc,
        x.Findings.OrderBy(y => y.CreatedAtUtc).Select(y => new ExamFailureFindingResponse(y.Id, y.Kind.ToString(), y.Code, y.Detail,
            y.Critical, y.Source, y.ActorUserId.Value, y.CreatedAtUtc)).ToArray());
}

public sealed class AddExamFailureFindingCommandHandler(IExamFailureAnalysisRepository repository, IExamsCertificationUnitOfWork uow, IClock clock)
    : ICommandHandler<AddExamFailureFindingCommand, ExamFailureAnalysisResponse>
{
    public async Task<Result<ExamFailureAnalysisResponse>> Handle(AddExamFailureFindingCommand c, CancellationToken ct)
    {
        ExamFailureAnalysis? x = await repository.GetByResultForUpdateAsync(c.OrganizationId, c.ResultId, c.ResultRevision, ct);
        if (x is null) return Result.Failure<ExamFailureAnalysisResponse>(ExamFailureAnalysisErrors.NotFound);
        if (!Enum.TryParse<ExamFailureFindingKind>(c.Kind, true, out var kind)) return Result.Failure<ExamFailureAnalysisResponse>(ExamFailureAnalysisErrors.InvalidFinding);
        Result r = x.AddFinding(kind, c.Code, c.Detail, c.Critical, c.Source, c.ActorUserId, clock.UtcNow);
        if (r.IsFailure) return Result.Failure<ExamFailureAnalysisResponse>(r.Error);
        await uow.CommitAsync(ct);
        return Result.Success(GetExamFailureAnalysisQueryHandler.Map(x));
    }
}

public sealed class UpdateExamFailureNarrativeCommandHandler(IExamFailureAnalysisRepository repository, IExamsCertificationUnitOfWork uow, IClock clock)
    : ICommandHandler<UpdateExamFailureNarrativeCommand, ExamFailureAnalysisResponse>
{
    public async Task<Result<ExamFailureAnalysisResponse>> Handle(UpdateExamFailureNarrativeCommand c, CancellationToken ct)
    {
        ExamFailureAnalysis? x = await repository.GetByResultForUpdateAsync(c.OrganizationId, c.ResultId, c.ResultRevision, ct);
        if (x is null) return Result.Failure<ExamFailureAnalysisResponse>(ExamFailureAnalysisErrors.NotFound);
        Result r = x.UpdateNarrative(c.InstructorAnalysis, c.StudentFeedback, c.Recommendation, c.ActorUserId, clock.UtcNow);
        if (r.IsFailure) return Result.Failure<ExamFailureAnalysisResponse>(r.Error);
        await uow.CommitAsync(ct);
        return Result.Success(GetExamFailureAnalysisQueryHandler.Map(x));
    }
}

public sealed class CompleteExamFailureAnalysisCommandHandler(IExamFailureAnalysisRepository repository, IExamsCertificationUnitOfWork uow, IClock clock)
    : ICommandHandler<CompleteExamFailureAnalysisCommand, ExamFailureAnalysisResponse>
{
    public async Task<Result<ExamFailureAnalysisResponse>> Handle(CompleteExamFailureAnalysisCommand c, CancellationToken ct)
    {
        ExamFailureAnalysis? x = await repository.GetByResultForUpdateAsync(c.OrganizationId, c.ResultId, c.ResultRevision, ct);
        if (x is null) return Result.Failure<ExamFailureAnalysisResponse>(ExamFailureAnalysisErrors.NotFound);
        Result r = x.Complete(c.Summary, c.Recommendation, c.ActorUserId, clock.UtcNow);
        if (r.IsFailure) return Result.Failure<ExamFailureAnalysisResponse>(r.Error);
        await uow.CommitAsync(ct);
        return Result.Success(GetExamFailureAnalysisQueryHandler.Map(x));
    }
}
