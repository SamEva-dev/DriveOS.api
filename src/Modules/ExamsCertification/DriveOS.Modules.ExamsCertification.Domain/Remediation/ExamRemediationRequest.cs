using DriveOS.Modules.ExamsCertification.Domain.Remediation.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Remediation;

/// <summary>
/// BC-11 aggregate coordinating the consequence of an approved failed-exam analysis.
/// It deliberately does not own the pedagogical remediation content: the detailed plan remains owned by BC-08.
/// </summary>
public sealed class ExamRemediationRequest : AggregateRoot<ExamRemediationRequestId>, IAuditableEntity
{
    private ExamRemediationRequest() { }

    private ExamRemediationRequest(ExamRemediationRequestId id, OrganizationId organizationId, ExamFailureAnalysisId analysisId,
        ExamResultId resultId, int resultRevision, ExamAttemptId failedAttemptId, ExamRegistrationId registrationId,
        PersonId studentId, int attemptNumber, TrainingPathId? trainingPathId, string analysisSummary,
        string? recommendationSummary, string affectedCompetencyIds, string recommendationCodes, int? recommendedHours,
        UserId actorUserId, DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId;
        FailureAnalysisId = analysisId;
        ExamResultId = resultId;
        ResultRevision = resultRevision;
        FailedAttemptId = failedAttemptId;
        RegistrationId = registrationId;
        StudentId = studentId;
        FailedAttemptNumber = attemptNumber;
        TrainingPathId = trainingPathId;
        AnalysisSummary = analysisSummary;
        RecommendationSummary = recommendationSummary;
        AffectedCompetencyIdsSerialized = affectedCompetencyIds;
        RecommendationCodesSerialized = recommendationCodes;
        RecommendedHours = recommendedHours;
        Status = trainingPathId.HasValue ? ExamRemediationRequestStatus.PendingConfiguration : ExamRemediationRequestStatus.Deferred;
        DeferredReasonCode = trainingPathId.HasValue ? null : "exams.remediation.training-path-unresolved";
        CreatedAtUtc = now.ToUniversalTime();
        CreatedByUserId = actorUserId;
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamFailureAnalysisId FailureAnalysisId { get; private set; }
    public ExamResultId ExamResultId { get; private set; }
    public int ResultRevision { get; private set; }
    public ExamAttemptId FailedAttemptId { get; private set; }
    public ExamRegistrationId RegistrationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public int FailedAttemptNumber { get; private set; }
    public TrainingPathId? TrainingPathId { get; private set; }
    public string AnalysisSummary { get; private set; } = string.Empty;
    public string? RecommendationSummary { get; private set; }
    public string AffectedCompetencyIdsSerialized { get; private set; } = string.Empty;
    public string RecommendationCodesSerialized { get; private set; } = string.Empty;
    public int? RecommendedHours { get; private set; }
    public UserId? ResponsibleUserId { get; private set; }
    public DateOnly? ReviewDate { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public bool MockExamRequired { get; private set; }
    public bool FundingReviewRequired { get; private set; }
    public RemediationPlanId? PedagogicalRemediationPlanId { get; private set; }
    public ExamRemediationRequestStatus Status { get; private set; }
    public string? DeferredReasonCode { get; private set; }
    public string? FailureCode { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTimeOffset? ProvisionedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? ValidatedForRePresentationAtUtc { get; private set; }
    public UserId? ValidatedByUserId { get; private set; }
    public DateTimeOffset? SupersededAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public IReadOnlyCollection<Guid> AffectedCompetencyIds => ParseGuids(AffectedCompetencyIdsSerialized);
    public IReadOnlyCollection<string> RecommendationCodes => Split(RecommendationCodesSerialized);

    public static Result<ExamRemediationRequest> Create(OrganizationId organizationId, ExamFailureAnalysisId analysisId,
        ExamResultId resultId, int resultRevision, ExamAttemptId failedAttemptId, ExamRegistrationId registrationId,
        PersonId studentId, int attemptNumber, TrainingPathId? trainingPathId, string analysisSummary,
        string? recommendationSummary, IReadOnlyCollection<Guid> affectedCompetencyIds,
        IReadOnlyCollection<string> recommendationCodes, int? recommendedHours, UserId actorUserId, DateTimeOffset now)
    {
        if (organizationId.IsEmpty || analysisId.IsEmpty || resultId.IsEmpty || failedAttemptId.IsEmpty || registrationId.IsEmpty || studentId.IsEmpty || actorUserId.IsEmpty || resultRevision <= 0 || attemptNumber <= 0 || string.IsNullOrWhiteSpace(analysisSummary))
            return Result.Failure<ExamRemediationRequest>(ExamRemediationRequestErrors.InvalidContext);
        if (recommendedHours is < 0 or > 200)
            return Result.Failure<ExamRemediationRequest>(ExamRemediationRequestErrors.InvalidRecommendedHours);

        var request = new ExamRemediationRequest(ExamRemediationRequestId.New(), organizationId, analysisId, resultId, resultRevision,
            failedAttemptId, registrationId, studentId, attemptNumber, trainingPathId, analysisSummary.Trim(), Normalize(recommendationSummary),
            string.Join(',', affectedCompetencyIds.Distinct().Order()), string.Join(',', recommendationCodes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase)),
            recommendedHours, actorUserId, now);
        request.RaiseDomainEvent(new ExamRemediationRequestedDomainEvent(request.Id, analysisId, resultId, resultRevision, failedAttemptId, organizationId, studentId));
        return Result.Success(request);
    }

    public Result Configure(TrainingPathId trainingPathId, UserId responsibleUserId, DateOnly reviewDate, DateOnly? targetDate,
        bool mockExamRequired, bool fundingReviewRequired, int? recommendedHours, UserId actorUserId, DateTimeOffset now)
    {
        if (Status == ExamRemediationRequestStatus.Superseded) return Result.Failure(ExamRemediationRequestErrors.Superseded);
        if (Status is ExamRemediationRequestStatus.Planned or ExamRemediationRequestStatus.InProgress or ExamRemediationRequestStatus.Completed or ExamRemediationRequestStatus.ValidatedForRePresentation)
            return Result.Failure(ExamRemediationRequestErrors.ProvisionNotAllowed);
        if (trainingPathId.IsEmpty || responsibleUserId.IsEmpty) return Result.Failure(ExamRemediationRequestErrors.ConfigurationRequired);
        if (reviewDate == default || reviewDate < DateOnly.FromDateTime(now.UtcDateTime.Date)) return Result.Failure(ExamRemediationRequestErrors.InvalidReviewDate);
        if (targetDate.HasValue && targetDate.Value < DateOnly.FromDateTime(now.UtcDateTime.Date)) return Result.Failure(ExamRemediationRequestErrors.InvalidReviewDate);
        if (targetDate.HasValue && targetDate.Value < reviewDate) return Result.Failure(ExamRemediationRequestErrors.InvalidReviewDate);
        if (recommendedHours is < 0 or > 200) return Result.Failure(ExamRemediationRequestErrors.InvalidRecommendedHours);
        TrainingPathId = trainingPathId;
        ResponsibleUserId = responsibleUserId;
        ReviewDate = reviewDate;
        TargetDate = targetDate;
        MockExamRequired = mockExamRequired;
        FundingReviewRequired = fundingReviewRequired;
        RecommendedHours = recommendedHours ?? RecommendedHours;
        DeferredReasonCode = null;
        FailureCode = null;
        Status = ExamRemediationRequestStatus.ReadyToProvision;
        Touch(actorUserId, now);
        return Result.Success();
    }

    public Result MarkProvisioning(UserId actorUserId, DateTimeOffset now)
    {
        if (Status != ExamRemediationRequestStatus.ReadyToProvision || !TrainingPathId.HasValue || !ResponsibleUserId.HasValue || !ReviewDate.HasValue)
            return Result.Failure(ExamRemediationRequestErrors.ProvisionNotAllowed);
        Status = ExamRemediationRequestStatus.Provisioning;
        Touch(actorUserId, now);
        return Result.Success();
    }

    public void MarkPlanned(RemediationPlanId planId, UserId actorUserId, DateTimeOffset now)
    {
        PedagogicalRemediationPlanId = planId;
        Status = ExamRemediationRequestStatus.Planned;
        ProvisionedAtUtc = now.ToUniversalTime();
        DeferredReasonCode = null;
        FailureCode = null;
        Touch(actorUserId, now);
        RaiseDomainEvent(new ExamRemediationPlanProvisionedDomainEvent(Id, planId, OrganizationId, StudentId));
    }

    public void MarkDeferred(string reasonCode, UserId actorUserId, DateTimeOffset now)
    {
        Status = ExamRemediationRequestStatus.Deferred;
        DeferredReasonCode = Normalize(reasonCode) ?? "exams.remediation.deferred";
        Touch(actorUserId, now);
    }

    public void MarkFailed(string failureCode, UserId actorUserId, DateTimeOffset now)
    {
        Status = ExamRemediationRequestStatus.Failed;
        FailureCode = Normalize(failureCode) ?? "exams.remediation.provision-failed";
        Touch(actorUserId, now);
    }

    public void SynchronizePedagogicalStatus(string pedagogicalStatus, UserId actorUserId, DateTimeOffset now)
    {
        if (Status is ExamRemediationRequestStatus.Superseded or ExamRemediationRequestStatus.Cancelled or ExamRemediationRequestStatus.ValidatedForRePresentation) return;
        switch (pedagogicalStatus)
        {
            case "Draft": Status = ExamRemediationRequestStatus.Planned; break;
            case "Active": Status = ExamRemediationRequestStatus.InProgress; break;
            case "Completed":
                Status = ExamRemediationRequestStatus.Completed;
                CompletedAtUtc ??= now.ToUniversalTime();
                if (PedagogicalRemediationPlanId.HasValue)
                    RaiseDomainEvent(new ExamRemediationCompletedDomainEvent(Id, PedagogicalRemediationPlanId.Value, OrganizationId, StudentId));
                break;
            case "Cancelled": Status = ExamRemediationRequestStatus.Cancelled; break;
        }
        Touch(actorUserId, now);
    }

    public Result ValidateForRePresentation(UserId actorUserId, DateTimeOffset now)
    {
        if (Status != ExamRemediationRequestStatus.Completed) return Result.Failure(ExamRemediationRequestErrors.ValidationNotAllowed);
        Status = ExamRemediationRequestStatus.ValidatedForRePresentation;
        ValidatedForRePresentationAtUtc = now.ToUniversalTime();
        ValidatedByUserId = actorUserId;
        Touch(actorUserId, now);
        RaiseDomainEvent(new ExamRemediationValidatedForRePresentationDomainEvent(Id, OrganizationId, StudentId, FailedAttemptId));
        return Result.Success();
    }

    public Result Cancel(string reason, UserId actorUserId, DateTimeOffset now)
    {
        if (Status is ExamRemediationRequestStatus.Completed or ExamRemediationRequestStatus.ValidatedForRePresentation or ExamRemediationRequestStatus.Superseded)
            return Result.Failure(ExamRemediationRequestErrors.CancelNotAllowed);
        CancellationReason = Normalize(reason);
        if (CancellationReason is null) return Result.Failure(ExamRemediationRequestErrors.CancelNotAllowed);
        Status = ExamRemediationRequestStatus.Cancelled;
        Touch(actorUserId, now);
        return Result.Success();
    }

    public void Supersede(DateTimeOffset now)
    {
        if (Status == ExamRemediationRequestStatus.Superseded) return;
        Status = ExamRemediationRequestStatus.Superseded;
        SupersededAtUtc = now.ToUniversalTime();
        RaiseDomainEvent(new ExamRemediationSupersededDomainEvent(Id, ExamResultId, ResultRevision, OrganizationId));
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;

        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }

    private void Touch(UserId actorUserId, DateTimeOffset now) => SetModifiedAudit(now, actorUserId);
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static IReadOnlyCollection<Guid> ParseGuids(string value) => Split(value).Select(Guid.Parse).ToArray();
    private static IReadOnlyCollection<string> Split(string value) => string.IsNullOrWhiteSpace(value) ? [] : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
