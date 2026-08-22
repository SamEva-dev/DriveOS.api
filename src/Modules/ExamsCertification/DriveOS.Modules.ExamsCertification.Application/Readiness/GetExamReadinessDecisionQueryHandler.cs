using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ExamsCertification.Domain.Readiness;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Readiness;

public sealed class GetExamReadinessDecisionQueryHandler(IExamReadinessDecisionRepository repository)
    : IQueryHandler<GetExamReadinessDecisionQuery, ExamReadinessDecisionResponse>
{
    public async Task<Result<ExamReadinessDecisionResponse>> Handle(
        GetExamReadinessDecisionQuery query,
        CancellationToken cancellationToken)
    {
        ExamReadinessDecision? decision = await repository.GetCurrentAsync(
            query.OrganizationId,
            query.StudentId,
            query.TrainingPathId,
            cancellationToken);

        if (decision is null)
            return Result.Failure<ExamReadinessDecisionResponse>(ExamReadinessApplicationErrors.DecisionNotFound);

        return Result.Success(new ExamReadinessDecisionResponse(
            decision.Id.Value,
            decision.StudentId.Value,
            decision.TrainingPathId.Value,
            decision.Version,
            decision.Outcome.ToString(),
            decision.PedagogicalCheck.ToString(),
            decision.AdministrativeCheck.ToString(),
            decision.FinancialCheck.ToString(),
            decision.RegulatoryCheck.ToString(),
            decision.Rationale,
            decision.Conditions,
            decision.ReviewerId.Value,
            decision.DecidedAtUtc,
            decision.IsCurrent,
            decision.SupersededByDecisionId?.Value,
            decision.SupersededAtUtc));
    }
}
