using DriveOS.Modules.ExamsCertification.Domain.Results.Failure.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Results.Failure;

/// <summary>
/// Human and structured analysis of one finalized failed exam-result revision. It never rewrites the official result and it does not own remediation.
/// </summary>
public sealed class ExamFailureAnalysis : AggregateRoot<ExamFailureAnalysisId>, IAuditableEntity
{
    private readonly List<ExamFailureFinding> _findings = [];
    private ExamFailureAnalysis() { }

    private ExamFailureAnalysis(ExamFailureAnalysisId id, OrganizationId organizationId, ExamResultId resultId, int resultRevision,
        ExamAttemptId attemptId, ExamRegistrationId registrationId, PersonId studentId, int attemptNumber,
        UserId actorUserId, DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId;
        ExamResultId = resultId;
        ResultRevision = resultRevision;
        AttemptId = attemptId;
        RegistrationId = registrationId;
        StudentId = studentId;
        AttemptNumber = attemptNumber;
        Status = ExamFailureAnalysisStatus.Draft;
        CreatedAtUtc = now.ToUniversalTime();
        CreatedByUserId = actorUserId;
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamResultId ExamResultId { get; private set; }
    public int ResultRevision { get; private set; }
    public ExamAttemptId AttemptId { get; private set; }
    public ExamRegistrationId RegistrationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public int AttemptNumber { get; private set; }
    public ExamFailureAnalysisStatus Status { get; private set; }
    public string? InstructorAnalysis { get; private set; }
    public string? StudentFeedback { get; private set; }
    public string? Summary { get; private set; }
    public string? Recommendation { get; private set; }
    public TrainingPathId? TrainingPathId { get; private set; }
    public string OfficialFailureReasonsSnapshot { get; private set; } = string.Empty;
    public string AffectedCompetencyIdsSerialized { get; private set; } = string.Empty;
    public string? FactualEvidence { get; private set; }
    public string ProbableCauseCodesSerialized { get; private set; } = string.Empty;
    public string? Hypotheses { get; private set; }
    public string RecommendationCodesSerialized { get; private set; } = string.Empty;
    public int? RecommendedHours { get; private set; }
    public DateTimeOffset? SubmittedAtUtc { get; private set; }
    public UserId? SubmittedByUserId { get; private set; }
    public DateTimeOffset? ApprovedAtUtc { get; private set; }
    public UserId? ApprovedByUserId { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public UserId? CompletedByUserId { get; private set; }
    public DateTimeOffset? SupersededAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }
    public IReadOnlyCollection<ExamFailureFinding> Findings => _findings.AsReadOnly();

    public static ExamFailureAnalysis Create(OrganizationId organizationId, ExamResultId resultId, int resultRevision,
        ExamAttemptId attemptId, ExamRegistrationId registrationId, PersonId studentId, int attemptNumber,
        string? officialFailureReasonCode, UserId actorUserId, DateTimeOffset now)
    {
        var analysis = new ExamFailureAnalysis(ExamFailureAnalysisId.New(), organizationId, resultId, resultRevision, attemptId,
            registrationId, studentId, attemptNumber, actorUserId, now);
        analysis.OfficialFailureReasonsSnapshot = Normalize(officialFailureReasonCode) ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(officialFailureReasonCode))
            analysis._findings.Add(new ExamFailureFinding(Guid.NewGuid(), ExamFailureFindingKind.OfficialFailureReason,
                officialFailureReasonCode.Trim(), null, true, "ExamResult", actorUserId, now));
        analysis.RaiseDomainEvent(new ExamFailureAnalysisCreatedDomainEvent(analysis.Id, resultId, resultRevision, attemptId, organizationId, studentId));
        return analysis;
    }

    public Result AddFinding(ExamFailureFindingKind kind, string code, string? detail, bool critical, string source, UserId actorUserId, DateTimeOffset now)
    {
        if (Status is ExamFailureAnalysisStatus.Approved) return Result.Failure(ExamFailureAnalysisErrors.AlreadyCompleted);
        if (Status is ExamFailureAnalysisStatus.Superseded) return Result.Failure(ExamFailureAnalysisErrors.Superseded);
        if (!Enum.IsDefined(kind) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(source)) return Result.Failure(ExamFailureAnalysisErrors.InvalidFinding);
        string normalized = code.Trim();
        if (_findings.Any(x => x.Kind == kind && string.Equals(x.Code, normalized, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure(ExamFailureAnalysisErrors.DuplicateFinding);
        _findings.Add(new ExamFailureFinding(Guid.NewGuid(), kind, normalized, detail, critical, source, actorUserId, now));
        Status = ExamFailureAnalysisStatus.UnderReview;
        Touch(actorUserId, now);
        RaiseDomainEvent(new ExamFailureFindingAddedDomainEvent(Id, kind, normalized));
        return Result.Success();
    }

    public Result UpdateNarrative(string? instructorAnalysis, string? studentFeedback, string? recommendation, UserId actorUserId, DateTimeOffset now)
    {
        if (Status is ExamFailureAnalysisStatus.Approved) return Result.Failure(ExamFailureAnalysisErrors.AlreadyCompleted);
        if (Status is ExamFailureAnalysisStatus.Superseded) return Result.Failure(ExamFailureAnalysisErrors.Superseded);
        InstructorAnalysis = Normalize(instructorAnalysis);
        StudentFeedback = Normalize(studentFeedback);
        Recommendation = Normalize(recommendation);
        Status = ExamFailureAnalysisStatus.UnderReview;
        Touch(actorUserId, now);
        return Result.Success();
    }

    public Result Complete(string summary, string? recommendation, UserId actorUserId, DateTimeOffset now)
    {
        if (Status is ExamFailureAnalysisStatus.Approved) return Result.Failure(ExamFailureAnalysisErrors.AlreadyCompleted);
        if (Status is ExamFailureAnalysisStatus.Superseded) return Result.Failure(ExamFailureAnalysisErrors.Superseded);
        if (_findings.Count == 0) return Result.Failure(ExamFailureAnalysisErrors.FindingsRequired);
        if (string.IsNullOrWhiteSpace(summary)) return Result.Failure(ExamFailureAnalysisErrors.SummaryRequired);
        Summary = summary.Trim();
        Recommendation = Normalize(recommendation) ?? Recommendation;
        Status = ExamFailureAnalysisStatus.Approved;
        SubmittedAtUtc ??= now.ToUniversalTime();
        ApprovedAtUtc = now.ToUniversalTime();
        ApprovedByUserId = actorUserId;
        CompletedAtUtc = now.ToUniversalTime();
        CompletedByUserId = actorUserId;
        Touch(actorUserId, now);
        RaiseDomainEvent(new ExamFailureAnalysisCompletedDomainEvent(Id, ExamResultId, ResultRevision, AttemptId, OrganizationId, StudentId, AttemptNumber));
        return Result.Success();
    }


    public Result UpdateStructuredAnalysis(TrainingPathId? trainingPathId, IReadOnlyCollection<Guid>? affectedCompetencyIds,
        string? factualEvidence, IReadOnlyCollection<ExamFailureCauseCode>? probableCauses, string? hypotheses,
        IReadOnlyCollection<ExamFailureRecommendationCode>? recommendationCodes, int? recommendedHours,
        UserId actorUserId, DateTimeOffset now)
    {
        if (Status is ExamFailureAnalysisStatus.Submitted or ExamFailureAnalysisStatus.Approved)
            return Result.Failure(ExamFailureAnalysisErrors.AlreadyCompleted);
        if (Status == ExamFailureAnalysisStatus.Superseded) return Result.Failure(ExamFailureAnalysisErrors.Superseded);
        if (recommendedHours is < 0 or > 200) return Result.Failure(ExamFailureAnalysisErrors.InvalidRecommendedHours);
        TrainingPathId = trainingPathId;
        AffectedCompetencyIdsSerialized = string.Join(',', (affectedCompetencyIds ?? []).Distinct().Order());
        FactualEvidence = Normalize(factualEvidence);
        ProbableCauseCodesSerialized = string.Join(',', (probableCauses ?? []).Distinct().OrderBy(x => (int)x));
        Hypotheses = Normalize(hypotheses);
        RecommendationCodesSerialized = string.Join(',', (recommendationCodes ?? []).Distinct().OrderBy(x => (int)x));
        RecommendedHours = recommendedHours;
        Status = ExamFailureAnalysisStatus.UnderReview;
        Touch(actorUserId, now);
        return Result.Success();
    }

    public Result Submit(UserId actorUserId, DateTimeOffset now)
    {
        if (Status == ExamFailureAnalysisStatus.Superseded) return Result.Failure(ExamFailureAnalysisErrors.Superseded);
        if (Status == ExamFailureAnalysisStatus.Approved) return Result.Failure(ExamFailureAnalysisErrors.AlreadyCompleted);
        if (_findings.Count == 0) return Result.Failure(ExamFailureAnalysisErrors.FindingsRequired);
        if (string.IsNullOrWhiteSpace(Summary) && string.IsNullOrWhiteSpace(InstructorAnalysis)) return Result.Failure(ExamFailureAnalysisErrors.SummaryRequired);
        Status = ExamFailureAnalysisStatus.Submitted; SubmittedAtUtc = now.ToUniversalTime(); SubmittedByUserId = actorUserId; Touch(actorUserId, now);
        RaiseDomainEvent(new ExamFailureAnalysisSubmittedDomainEvent(Id, ExamResultId, ResultRevision, AttemptId, OrganizationId, StudentId));
        return Result.Success();
    }

    public Result Approve(string summary, string? recommendation, UserId actorUserId, DateTimeOffset now)
    {
        if (Status != ExamFailureAnalysisStatus.Submitted) return Result.Failure(ExamFailureAnalysisErrors.NotSubmitted);
        if (string.IsNullOrWhiteSpace(summary)) return Result.Failure(ExamFailureAnalysisErrors.SummaryRequired);
        Summary = summary.Trim(); Recommendation = Normalize(recommendation) ?? Recommendation;
        Status = ExamFailureAnalysisStatus.Approved; ApprovedAtUtc = now.ToUniversalTime(); ApprovedByUserId = actorUserId; CompletedAtUtc = ApprovedAtUtc; CompletedByUserId = actorUserId; Touch(actorUserId, now);
        RaiseDomainEvent(new ExamFailureAnalysisApprovedDomainEvent(Id, ExamResultId, ResultRevision, AttemptId, OrganizationId, StudentId, AttemptNumber));
        return Result.Success();
    }

    public void Supersede(DateTimeOffset now)
    {
        if (Status == ExamFailureAnalysisStatus.Superseded) return;
        Status = ExamFailureAnalysisStatus.Superseded;
        SupersededAtUtc = now.ToUniversalTime();
        RaiseDomainEvent(new ExamFailureAnalysisSupersededDomainEvent(Id, ExamResultId, ResultRevision, OrganizationId));
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
}
