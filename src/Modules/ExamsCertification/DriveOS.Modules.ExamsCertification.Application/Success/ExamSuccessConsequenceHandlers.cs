using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Success;

public sealed class GetExamSuccessConsequencesQueryHandler(IExamSuccessConsequenceStore store)
    : IQueryHandler<GetExamSuccessConsequencesQuery, IReadOnlyList<ExamSuccessConsequenceResponse>>
{
    public async Task<Result<IReadOnlyList<ExamSuccessConsequenceResponse>>> Handle(GetExamSuccessConsequencesQuery query, CancellationToken cancellationToken)
        => Result.Success<IReadOnlyList<ExamSuccessConsequenceResponse>>(Map(await store.GetByResultAsync(query.OrganizationId, query.ResultId, cancellationToken)));

    internal static IReadOnlyList<ExamSuccessConsequenceResponse> Map(IReadOnlyList<ExamSuccessConsequenceEnvelope> items) =>
        items.Select(x => new ExamSuccessConsequenceResponse(x.Id, x.Kind.ToString(), x.Status.ToString(), x.AttemptCount,
            x.CreatedAtUtc, x.LastAttemptAtUtc, x.NextAttemptAtUtc, x.ProcessedAtUtc, x.SupersededAtUtc,
            x.LastErrorCode, x.LastErrorDetail)).ToArray();
}

public sealed class RequeueExamSuccessConsequencesCommandHandler(IExamSuccessConsequenceStore store, IClock clock)
    : ICommandHandler<RequeueExamSuccessConsequencesCommand, IReadOnlyList<ExamSuccessConsequenceResponse>>
{
    public async Task<Result<IReadOnlyList<ExamSuccessConsequenceResponse>>> Handle(RequeueExamSuccessConsequencesCommand command, CancellationToken cancellationToken)
    {
        await store.RequeueAsync(command.OrganizationId, command.ResultId, clock.UtcNow, cancellationToken);
        return Result.Success<IReadOnlyList<ExamSuccessConsequenceResponse>>(
            GetExamSuccessConsequencesQueryHandler.Map(await store.GetByResultAsync(command.OrganizationId, command.ResultId, cancellationToken)));
    }
}
