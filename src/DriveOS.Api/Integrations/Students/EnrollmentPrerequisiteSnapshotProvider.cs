using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Read;
using DriveOS.Modules.CRM.Application.Assessments.GetAssessments;
using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.Modules.FundingBilling.Application.StudentFinance.Read;
using DriveOS.Modules.Students.Application.Checklists;
using DriveOS.Modules.Students.Domain.Checklists;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.Students;

/// <summary>
/// Composition-layer adapter. Student Administration owns the checklist, while each
/// prerequisite remains owned by its bounded context. A failing source must not make
/// the whole student file unavailable; unknown prerequisites keep their persisted state.
/// </summary>
internal sealed class EnrollmentPrerequisiteSnapshotProvider(
    ITrainingContractReadService contractReadService,
    ITrainingPathReadService trainingPathReadService,
    IStudentFinancialOverviewReadService financialOverviewReadService,
    IMediator mediator,
    IClock clock,
    ILogger<EnrollmentPrerequisiteSnapshotProvider> logger)
    : IEnrollmentPrerequisiteSnapshotProvider
{
    public async Task<EnrollmentPrerequisiteSnapshot> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        DraftEnrollmentId enrollmentId,
        LeadId? sourceLeadId,
        CancellationToken cancellationToken = default)
    {
        PrerequisiteEvaluation? contract = await SafeAsync(
            "Contracts",
            async () => EvaluateContract(
                await contractReadService.ListAsync(organizationId, studentId, cancellationToken)));

        PrerequisiteEvaluation? finance = await SafeAsync(
            "FundingBilling",
            async () => EvaluateFinance(
                await financialOverviewReadService.GetAsync(
                    organizationId,
                    studentId,
                    DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),
                    cancellationToken)));

        PrerequisiteEvaluation? learningPath = await SafeAsync(
            "CurriculumPedagogy",
            async () => EvaluateLearningPath(
                await trainingPathReadService.ListForStudentAsync(
                    organizationId,
                    studentId,
                    cancellationToken)));

        PrerequisiteEvaluation? assessment = sourceLeadId.HasValue
            ? await SafeAsync(
                "CRM",
                async () => EvaluateAssessment(
                    await mediator.Send(
                        new GetLeadAssessmentsQuery(organizationId, sourceLeadId.Value),
                        cancellationToken)))
            : null;

        return new EnrollmentPrerequisiteSnapshot(
            Contract: contract,
            InitialPayment: finance,
            InitialAssessment: assessment,
            LearningPath: learningPath,
            StudentAccount: null);
    }

    private async Task<PrerequisiteEvaluation?> SafeAsync(
        string source,
        Func<Task<PrerequisiteEvaluation>> read)
    {
        try
        {
            return await read();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Enrollment checklist prerequisite source {Source} is unavailable. The persisted checklist state will be preserved.",
                source);
            return null;
        }
    }

    private static PrerequisiteEvaluation EvaluateContract(
        IReadOnlyCollection<TrainingContractListItemResponse> contracts)
    {
        TrainingContractListItemResponse? current = contracts
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefault();
        if (current is null)
            return new PrerequisiteEvaluation(ChecklistItemStatus.NotStarted);

        return current.Status switch
        {
            "Signed" or "Active" or "Amended" or "Completed" =>
                new PrerequisiteEvaluation(
                    ChecklistItemStatus.Completed,
                    $"training-contract:{current.Id}"),
            "Terminated" or "Cancelled" or "Expired" =>
                new PrerequisiteEvaluation(
                    ChecklistItemStatus.Blocked,
                    $"training-contract:{current.Id}"),
            _ => new PrerequisiteEvaluation(
                ChecklistItemStatus.InProgress,
                $"training-contract:{current.Id}"),
        };
    }

    private static PrerequisiteEvaluation EvaluateFinance(StudentFinancialOverviewResponse? finance)
    {
        if (finance is null)
            return new PrerequisiteEvaluation(ChecklistItemStatus.NotStarted);

        bool paymentReceived = finance.Totals.TotalPaid > 0m;
        StudentFinancialFundingPlanSummaryResponse? approvedFunding = finance.FundingPlans
            .FirstOrDefault(x => string.Equals(x.Status, "Approved", StringComparison.OrdinalIgnoreCase));

        if (paymentReceived || approvedFunding is not null)
        {
            string evidence = paymentReceived
                ? $"billing-account:{finance.BillingAccountId}:payment"
                : $"funding-plan:{approvedFunding!.Id}";
            return new PrerequisiteEvaluation(ChecklistItemStatus.Completed, evidence);
        }

        bool pending = finance.FundingPlans.Any(x =>
                x.Status is "Draft" or "PendingApproval" or "PartiallyApproved")
            || finance.RecentPayments.Any(x => x.Status is "Pending" or "Processing");

        return new PrerequisiteEvaluation(
            pending ? ChecklistItemStatus.InProgress : ChecklistItemStatus.NotStarted,
            $"billing-account:{finance.BillingAccountId}");
    }

    private static PrerequisiteEvaluation EvaluateAssessment(
        Result<IReadOnlyList<AssessmentAppointmentResponse>> result)
    {
        if (result.IsFailure)
            return new PrerequisiteEvaluation(ChecklistItemStatus.NotStarted);

        AssessmentAppointmentResponse? completed = result.Value
            .OrderByDescending(x => x.ClosedAtUtc ?? x.CreatedAtUtc)
            .FirstOrDefault(x => string.Equals(x.Status, "Completed", StringComparison.OrdinalIgnoreCase));
        if (completed is not null)
            return new PrerequisiteEvaluation(
                ChecklistItemStatus.Completed,
                $"assessment:{completed.Id}");

        bool planned = result.Value.Any(x =>
            x.Status is "Scheduled" or "Confirmed" or "Rescheduled");
        return new PrerequisiteEvaluation(
            planned ? ChecklistItemStatus.InProgress : ChecklistItemStatus.NotStarted);
    }

    private static PrerequisiteEvaluation EvaluateLearningPath(
        IReadOnlyCollection<TrainingPathListItem> paths)
    {
        TrainingPathListItem? path = paths.OrderByDescending(x => x.CreatedAtUtc).FirstOrDefault();
        if (path is null)
            return new PrerequisiteEvaluation(ChecklistItemStatus.NotStarted);

        return path.Status switch
        {
            "ReadyForActivation" or "Active" or "Completed" =>
                new PrerequisiteEvaluation(
                    ChecklistItemStatus.Completed,
                    $"training-path:{path.Id}"),
            "Suspended" or "Cancelled" =>
                new PrerequisiteEvaluation(
                    ChecklistItemStatus.Blocked,
                    $"training-path:{path.Id}"),
            _ => new PrerequisiteEvaluation(
                ChecklistItemStatus.InProgress,
                $"training-path:{path.Id}"),
        };
    }
}
