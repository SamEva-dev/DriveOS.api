using DomainRelay.Abstractions;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Complete;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Read;
using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.Modules.ExamsCertification.Application.Success;
using DriveOS.Modules.FundingBilling.Application.StudentFinance.Read;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.ExamsCertification;

internal sealed class ExamSuccessConsequenceGateway(
    IMediator mediator,
    ITrainingPathReadService trainingPaths,
    ITrainingContractReadService trainingContracts,
    IStudentFinancialOverviewReadService financialOverview) : IExamSuccessConsequenceGateway
{
    public async Task<ExamSuccessConsequenceDispatchResult> DispatchAsync(ExamSuccessConsequenceEnvelope consequence, CancellationToken cancellationToken = default)
    {
        ExamSuccessSnapshot s = consequence.Snapshot;
        switch (consequence.Kind)
        {
            case ExamSuccessConsequenceKind.PedagogicalCompletion:
            {
                IReadOnlyCollection<TrainingPathListItem> paths = await trainingPaths.ListForStudentAsync(s.OrganizationId, s.StudentId, cancellationToken);
                TrainingPathListItem[] matching = paths.Where(x => string.Equals(x.LicenseCategoryCode, s.LicenseCategory, StringComparison.OrdinalIgnoreCase)
                    && x.Status is "Active" or "Suspended").ToArray();
                if (matching.Length == 0)
                {
                    if (paths.Any(x => string.Equals(x.LicenseCategoryCode, s.LicenseCategory, StringComparison.OrdinalIgnoreCase) && x.Status == "Completed"))
                        return ExamSuccessConsequenceDispatchResult.Processed();
                    return ExamSuccessConsequenceDispatchResult.Deferred("exam-success.training-path.not-resolved", s.LicenseCategory);
                }
                if (matching.Length > 1)
                    return ExamSuccessConsequenceDispatchResult.Deferred("exam-success.training-path.ambiguous", $"count={matching.Length}");

                var result = await mediator.Send(new CompleteTrainingPathCommand(s.OrganizationId, new TrainingPathId(matching[0].Id), s.FinalizedByUserId), cancellationToken);
                return result.IsSuccess
                    ? ExamSuccessConsequenceDispatchResult.Processed()
                    : ExamSuccessConsequenceDispatchResult.Retry(result.Error.Code, result.Error.MessageKey);
            }

            case ExamSuccessConsequenceKind.ContractCompletion:
            {
                IReadOnlyList<TrainingContractListItemResponse> contracts = await trainingContracts.ListAsync(s.OrganizationId, s.StudentId, cancellationToken);
                TrainingContractListItemResponse[] matching = contracts.Where(x => string.Equals(x.TrainingCode, s.LicenseCategory, StringComparison.OrdinalIgnoreCase)
                    && x.Status is "Active" or "Signed" or "Amended").ToArray();
                if (matching.Length == 0)
                {
                    if (contracts.Any(x => string.Equals(x.TrainingCode, s.LicenseCategory, StringComparison.OrdinalIgnoreCase) && x.Status == "Completed"))
                        return ExamSuccessConsequenceDispatchResult.Processed();
                    return ExamSuccessConsequenceDispatchResult.Deferred("exam-success.training-contract.not-resolved", s.LicenseCategory);
                }
                if (matching.Length > 1)
                    return ExamSuccessConsequenceDispatchResult.Deferred("exam-success.training-contract.ambiguous", $"count={matching.Length}");

                var result = await mediator.Send(new CompleteTrainingContractCommand(s.OrganizationId, new TrainingContractId(matching[0].Id),
                    $"Exam passed. result={s.ResultId.Value:N}; attempt={s.AttemptId.Value:N}; revision={s.ResultRevision}",
                    DateOnly.FromDateTime(s.ResultFinalizedAtUtc.UtcDateTime), s.FinalizedByUserId), cancellationToken);
                return result.IsSuccess
                    ? ExamSuccessConsequenceDispatchResult.Processed()
                    : ExamSuccessConsequenceDispatchResult.Retry(result.Error.Code, result.Error.MessageKey);
            }

            case ExamSuccessConsequenceKind.FinancialClosureReview:
            {
                StudentFinancialOverviewResponse? finance = await financialOverview.GetAsync(s.OrganizationId, s.StudentId,
                    DateOnly.FromDateTime(s.ResultFinalizedAtUtc.UtcDateTime), cancellationToken);
                if (finance is null)
                    return ExamSuccessConsequenceDispatchResult.Deferred("exam-success.finance.account-not-resolved");
                if (finance.Totals.OutstandingBalance > 0 || finance.Totals.OverdueAmount > 0 || finance.Alerts.HasFinancialBlock)
                    return ExamSuccessConsequenceDispatchResult.Deferred("exam-success.finance.closure-pending",
                        $"outstanding={finance.Totals.OutstandingBalance};overdue={finance.Totals.OverdueAmount};blocked={finance.Alerts.HasFinancialBlock}");
                return ExamSuccessConsequenceDispatchResult.Processed();
            }

            case ExamSuccessConsequenceKind.StudentJourneyTransition:
                return ExamSuccessConsequenceDispatchResult.Deferred("students.exam-success-transition.requires-dedicated-contract");
            case ExamSuccessConsequenceKind.CertificationEligibility:
                return ExamSuccessConsequenceDispatchResult.Deferred("exams.certification.exm019-pending");
            case ExamSuccessConsequenceKind.SchedulingFollowUpReview:
                return ExamSuccessConsequenceDispatchResult.Deferred("scheduling.exam-success-future-bookings.review-required");
            case ExamSuccessConsequenceKind.SuccessCommunication:
                return ExamSuccessConsequenceDispatchResult.Deferred("communication.exam-success.integration-pending");
            case ExamSuccessConsequenceKind.AnalyticsMetrics:
                // EXM-020 is a transactional read-side derived from authoritative attempts/results.
                // No mutable projection needs to be synchronized here.
                return ExamSuccessConsequenceDispatchResult.Processed();
            default:
                return ExamSuccessConsequenceDispatchResult.PermanentFailure("exam-success.consequence-kind-unsupported");
        }
    }
}
