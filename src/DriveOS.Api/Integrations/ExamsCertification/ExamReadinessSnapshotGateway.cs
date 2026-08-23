using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Read;
using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using DriveOS.Modules.CurriculumPedagogy.Application.Readiness;
using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.Modules.ExamsCertification.Application.Readiness;
using DriveOS.Modules.ExamsCertification.Domain.Readiness;
using DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;
using DriveOS.Modules.FundingBilling.Application.StudentFinance.Read;
using DriveOS.Modules.Students.Application.Statuses;
using DriveOS.Modules.Students.Domain.Administration;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Statuses;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Api.Integrations.ExamsCertification;

internal sealed class ExamReadinessSnapshotGateway(
    IStudentStatusService studentStatuses,
    IPedagogicalReadinessReadService pedagogy,
    IStudentFinancialOverviewReadService finance,
    ITrainingContractReadService contracts,
    TrainingDeliveryDbContext trainingDelivery,
    IExamReadinessOpinionRepository opinions,
    ITrainingPathReadService trainingPaths,
    IRegulatoryTrainingRecordGateway regulatoryTrainingRecords,
    IClock clock) : IExamReadinessSnapshotGateway
{
    public async Task<Result<ExamReadinessSnapshot>> EvaluateAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        CancellationToken cancellationToken = default)
    {
        var checks = new List<ExamReadinessSourceCheck>();

        PedagogicalReadinessCheckResponse? pedagogical = await pedagogy.GetAsync(
            organizationId,
            trainingPathId,
            cancellationToken);

        if (pedagogical is null)
            return Result.Failure<ExamReadinessSnapshot>(ExamReadinessApplicationErrors.TrainingPathNotFound);

        if (pedagogical.StudentId != studentId.Value)
            return Result.Failure<ExamReadinessSnapshot>(ExamReadinessApplicationErrors.TrainingPathStudentMismatch);

        StudentStatusesResponse? student = await studentStatuses.GetAsync(
            organizationId,
            studentId,
            cancellationToken);

        if (student is null)
            return Result.Failure<ExamReadinessSnapshot>(ExamReadinessApplicationErrors.StudentNotFound);

        IReadOnlyList<ExamReadinessOpinion> pedagogicalOpinions = await opinions.ListAsync(
            organizationId,
            studentId,
            trainingPathId,
            cancellationToken);

        ExamReadinessCheckStatus pedagogicalStatus = EvaluatePedagogy(pedagogical, pedagogicalOpinions, checks);
        ExamReadinessCheckStatus administrativeStatus = await EvaluateAdministrationAsync(
            organizationId,
            studentId,
            student,
            contracts,
            checks,
            cancellationToken);
        ExamReadinessCheckStatus financialStatus = await EvaluateFinanceAsync(
            organizationId,
            studentId,
            finance,
            clock,
            checks,
            cancellationToken);
        TrainingPathDetailResponse? trainingPath = await trainingPaths.GetAsync(
            organizationId,
            trainingPathId,
            cancellationToken);

        if (trainingPath is null || trainingPath.StudentId != studentId.Value)
            return Result.Failure<ExamReadinessSnapshot>(ExamReadinessApplicationErrors.TrainingPathNotFound);

        ExamReadinessCheckStatus regulatoryStatus = await EvaluateRegulatoryAsync(
            organizationId,
            studentId,
            trainingPathId,
            student,
            trainingPath,
            regulatoryTrainingRecords,
            checks,
            cancellationToken);

        var delivery = await trainingDelivery.TrainingSessions
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId
                && x.StudentId == studentId
                && x.TrainingPathId == trainingPathId
                && x.Status == TrainingSessionStatus.Completed)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Minutes = g.Sum(x => x.DeliveredDurationMinutes ?? 0)
            })
            .SingleOrDefaultAsync(cancellationToken);

        int completedSessions = delivery?.Count ?? 0;
        decimal deliveredMinutes = delivery?.Minutes ?? 0;
        checks.Add(new ExamReadinessSourceCheck(
            "training-delivery-evidence",
            "exams.readiness.trainingDelivery.evidence",
            completedSessions > 0 ? ExamReadinessCheckStatus.Satisfied : ExamReadinessCheckStatus.Warning,
            "TrainingDelivery",
            $"sessions={completedSessions};minutes={deliveredMinutes}"));

        return Result.Success(new ExamReadinessSnapshot(
            studentId.Value,
            trainingPathId.Value,
            clock.UtcNow,
            pedagogicalStatus,
            administrativeStatus,
            financialStatus,
            regulatoryStatus,
            completedSessions,
            deliveredMinutes,
            checks));
    }

    private static ExamReadinessCheckStatus EvaluatePedagogy(
        PedagogicalReadinessCheckResponse value,
        IReadOnlyList<ExamReadinessOpinion> opinions,
        ICollection<ExamReadinessSourceCheck> checks)
    {
        ExamReadinessCheckStatus sourceStatus;
        if (value.Blockers.Count > 0)
            sourceStatus = ExamReadinessCheckStatus.Blocked;
        else if (value.LatestDecision is null)
            sourceStatus = ExamReadinessCheckStatus.Warning;
        else
            sourceStatus = value.LatestDecision.Decision switch
            {
                "Ready" => ExamReadinessCheckStatus.Satisfied,
                "ReadyWithConditions" => ExamReadinessCheckStatus.Warning,
                _ => ExamReadinessCheckStatus.Blocked
            };

        checks.Add(new ExamReadinessSourceCheck(
            "pedagogical-readiness",
            sourceStatus switch
            {
                ExamReadinessCheckStatus.Satisfied => "exams.readiness.pedagogy.satisfied",
                ExamReadinessCheckStatus.Warning => "exams.readiness.pedagogy.reviewRequired",
                _ => "exams.readiness.pedagogy.blocked"
            },
            sourceStatus,
            "CurriculumPedagogy",
            value.Blockers.Count == 0 ? null : string.Join(",", value.Blockers)));

        ExamReadinessOpinion[] latestByAuthor = opinions
            .GroupBy(x => x.AuthorId)
            .Select(g => g.OrderByDescending(x => x.Version).First())
            .ToArray();

        ExamReadinessCheckStatus opinionStatus;
        if (latestByAuthor.Length == 0)
            opinionStatus = ExamReadinessCheckStatus.Warning;
        else if (latestByAuthor.Any(x => x.Opinion == ExamReadinessOpinionType.Unfavorable))
            opinionStatus = ExamReadinessCheckStatus.Blocked;
        else if (latestByAuthor.Any(x => x.Opinion is ExamReadinessOpinionType.InsufficientData or ExamReadinessOpinionType.SecondOpinionRequested or ExamReadinessOpinionType.FavorableWithReservations))
            opinionStatus = ExamReadinessCheckStatus.Warning;
        else if (latestByAuthor.Any(x => x.Opinion == ExamReadinessOpinionType.Favorable))
            opinionStatus = ExamReadinessCheckStatus.Satisfied;
        else
            opinionStatus = ExamReadinessCheckStatus.Warning;

        checks.Add(new ExamReadinessSourceCheck(
            "pedagogical-opinion",
            opinionStatus switch
            {
                ExamReadinessCheckStatus.Satisfied => "exams.readiness.opinion.favorable",
                ExamReadinessCheckStatus.Blocked => "exams.readiness.opinion.unfavorable",
                _ => latestByAuthor.Length == 0
                    ? "exams.readiness.opinion.missing"
                    : "exams.readiness.opinion.requiresReview"
            },
            opinionStatus,
            "ExamsCertification",
            latestByAuthor.Length == 0
                ? null
                : string.Join(';', latestByAuthor.Select(x => $"{x.AuthorId.Value}:{x.Opinion}:v{x.Version}"))));

        if (sourceStatus == ExamReadinessCheckStatus.Blocked || opinionStatus == ExamReadinessCheckStatus.Blocked)
            return ExamReadinessCheckStatus.Blocked;
        if (sourceStatus == ExamReadinessCheckStatus.Satisfied && opinionStatus == ExamReadinessCheckStatus.Satisfied)
            return ExamReadinessCheckStatus.Satisfied;
        return ExamReadinessCheckStatus.Warning;
    }

    private static async Task<ExamReadinessCheckStatus> EvaluateAdministrationAsync(
        OrganizationId organizationId,
        PersonId studentId,
        StudentStatusesResponse student,
        ITrainingContractReadService contracts,
        ICollection<ExamReadinessSourceCheck> checks,
        CancellationToken cancellationToken)
    {
        bool enrollmentActive = student.EnrollmentStatus == EnrollmentStatus.Active;
        bool adminCompliant = student.AdministrativeStatus == AdministrativeStatus.Compliant;
        bool examBlocked = student.CurrentlyBlockedActions.HasFlag(StudentBlockingAction.PresentExam);

        IReadOnlyList<TrainingContractListItemResponse> studentContracts = await contracts.ListAsync(
            organizationId,
            studentId,
            cancellationToken);

        bool hasEligibleContract = studentContracts.Any(x =>
            x.Status is "Signed" or "Active" or "Amended" or "Completed");

        ExamReadinessCheckStatus status =
            !enrollmentActive || !adminCompliant || examBlocked || !hasEligibleContract
                ? ExamReadinessCheckStatus.Blocked
                : ExamReadinessCheckStatus.Satisfied;

        checks.Add(new ExamReadinessSourceCheck(
            "student-administrative-status",
            status == ExamReadinessCheckStatus.Satisfied
                ? "exams.readiness.administration.satisfied"
                : "exams.readiness.administration.blocked",
            status,
            "Students",
            $"enrollmentActive={enrollmentActive};administrativeCompliant={adminCompliant};presentExamBlocked={examBlocked}"));

        checks.Add(new ExamReadinessSourceCheck(
            "training-contract",
            hasEligibleContract
                ? "exams.readiness.contract.satisfied"
                : "exams.readiness.contract.required",
            hasEligibleContract ? ExamReadinessCheckStatus.Satisfied : ExamReadinessCheckStatus.Blocked,
            "Contracts"));

        return status;
    }

    private static async Task<ExamReadinessCheckStatus> EvaluateFinanceAsync(
        OrganizationId organizationId,
        PersonId studentId,
        IStudentFinancialOverviewReadService finance,
        IClock clock,
        ICollection<ExamReadinessSourceCheck> checks,
        CancellationToken cancellationToken)
    {
        StudentFinancialOverviewResponse? financial = await finance.GetAsync(
            organizationId,
            studentId,
            DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),
            cancellationToken);

        if (financial is null)
        {
            checks.Add(new ExamReadinessSourceCheck(
                "student-finance",
                "exams.readiness.finance.unavailable",
                ExamReadinessCheckStatus.Warning,
                "FundingBilling"));
            return ExamReadinessCheckStatus.Warning;
        }

        ExamReadinessCheckStatus status = financial.Alerts.HasFinancialBlock
            ? ExamReadinessCheckStatus.Blocked
            : financial.Totals.OverdueAmount > 0m
                ? ExamReadinessCheckStatus.Warning
                : ExamReadinessCheckStatus.Satisfied;

        checks.Add(new ExamReadinessSourceCheck(
            "student-finance",
            status switch
            {
                ExamReadinessCheckStatus.Satisfied => "exams.readiness.finance.satisfied",
                ExamReadinessCheckStatus.Warning => "exams.readiness.finance.warning",
                _ => "exams.readiness.finance.blocked"
            },
            status,
            "FundingBilling",
            $"outstanding={financial.Totals.OutstandingBalance};overdue={financial.Totals.OverdueAmount}"));

        return status;
    }

    private static async Task<ExamReadinessCheckStatus> EvaluateRegulatoryAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        StudentStatusesResponse student,
        TrainingPathDetailResponse trainingPath,
        IRegulatoryTrainingRecordGateway regulatoryTrainingRecords,
        ICollection<ExamReadinessSourceCheck> checks,
        CancellationToken cancellationToken)
    {
        bool explicitExamBlock = student.CurrentlyBlockedActions.HasFlag(StudentBlockingAction.PresentExam);
        if (explicitExamBlock)
        {
            checks.Add(new ExamReadinessSourceCheck(
                "regulatory-training-record",
                "exams.readiness.regulatory.blocked",
                ExamReadinessCheckStatus.Blocked,
                "RegulatoryTrainingRecord",
                "Student PresentExam block is active."));
            return ExamReadinessCheckStatus.Blocked;
        }

        Result<RegulatoryTrainingRecordEvaluation> evaluationResult = await regulatoryTrainingRecords.EvaluateAsync(
            new RegulatoryTrainingRecordContext(
                organizationId,
                studentId,
                trainingPathId,
                trainingPath.CountryCode,
                "Readiness",
                trainingPath.LicenseCategoryCode),
            cancellationToken);

        if (evaluationResult.IsFailure)
        {
            checks.Add(new ExamReadinessSourceCheck(
                "regulatory-training-record",
                "exams.readiness.regulatory.unavailable",
                ExamReadinessCheckStatus.Warning,
                "RegulatoryTrainingRecord",
                $"error={evaluationResult.Error.Code}"));
            return ExamReadinessCheckStatus.Warning;
        }

        RegulatoryTrainingRecordEvaluation evaluation = evaluationResult.Value;
        ExamReadinessCheckStatus status = evaluation.Required
            ? evaluation.Status switch
            {
                RegulatoryTrainingRecordStatus.Compliant => ExamReadinessCheckStatus.Satisfied,
                RegulatoryTrainingRecordStatus.Blocked => ExamReadinessCheckStatus.Blocked,
                RegulatoryTrainingRecordStatus.Warning => ExamReadinessCheckStatus.Warning,
                RegulatoryTrainingRecordStatus.NotApplicable => ExamReadinessCheckStatus.NotApplicable,
                _ => ExamReadinessCheckStatus.Warning
            }
            : evaluation.Status == RegulatoryTrainingRecordStatus.NotApplicable
                ? ExamReadinessCheckStatus.NotApplicable
                : ExamReadinessCheckStatus.Unknown;

        string messageKey = status switch
        {
            ExamReadinessCheckStatus.Satisfied => "exams.readiness.regulatory.satisfied",
            ExamReadinessCheckStatus.Blocked => "exams.readiness.regulatory.blocked",
            ExamReadinessCheckStatus.NotApplicable => "exams.readiness.regulatory.notApplicable",
            ExamReadinessCheckStatus.Warning when evaluation.Status == RegulatoryTrainingRecordStatus.Warning => "exams.readiness.regulatory.warning",
            ExamReadinessCheckStatus.Warning => "exams.readiness.regulatory.pending",
            _ => "exams.readiness.regulatory.providerPending"
        };

        checks.Add(new ExamReadinessSourceCheck(
            "regulatory-training-record",
            messageKey,
            status,
            "RegulatoryTrainingRecord",
            $"required={evaluation.Required.ToString().ToLowerInvariant()};provider={evaluation.ProviderCode};status={evaluation.Status};externalReference={evaluation.ExternalReference ?? "none"};{evaluation.Evidence}"));

        return status;
    }
}
