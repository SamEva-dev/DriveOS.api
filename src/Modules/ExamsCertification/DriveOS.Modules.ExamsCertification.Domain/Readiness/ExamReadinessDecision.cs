using DriveOS.Modules.ExamsCertification.Domain.Readiness.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Readiness;

/// <summary>
/// Authoritative, versioned decision determining whether a student may be presented for an exam.
/// It stores a snapshot of the four independent readiness dimensions without becoming the source
/// of truth for pedagogy, student administration, finance, or national regulatory systems.
/// </summary>
public sealed class ExamReadinessDecision : AggregateRoot<ExamReadinessDecisionId>, IAuditableEntity
{
    private ExamReadinessDecision() { }

    private ExamReadinessDecision(
        ExamReadinessDecisionId id,
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        int version,
        ExamReadinessOutcome outcome,
        ExamReadinessCheckStatus pedagogicalCheck,
        ExamReadinessCheckStatus administrativeCheck,
        ExamReadinessCheckStatus financialCheck,
        ExamReadinessCheckStatus regulatoryCheck,
        string rationale,
        string? conditions,
        UserId reviewerId,
        DateTimeOffset decidedAtUtc) : base(id)
    {
        OrganizationId = organizationId;
        StudentId = studentId;
        TrainingPathId = trainingPathId;
        Version = version;
        Outcome = outcome;
        PedagogicalCheck = pedagogicalCheck;
        AdministrativeCheck = administrativeCheck;
        FinancialCheck = financialCheck;
        RegulatoryCheck = regulatoryCheck;
        Rationale = rationale;
        Conditions = conditions;
        ReviewerId = reviewerId;
        DecidedAtUtc = decidedAtUtc.ToUniversalTime();
        IsCurrent = true;
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public TrainingPathId TrainingPathId { get; private set; }
    public int Version { get; private set; }
    public ExamReadinessOutcome Outcome { get; private set; }
    public ExamReadinessCheckStatus PedagogicalCheck { get; private set; }
    public ExamReadinessCheckStatus AdministrativeCheck { get; private set; }
    public ExamReadinessCheckStatus FinancialCheck { get; private set; }
    public ExamReadinessCheckStatus RegulatoryCheck { get; private set; }
    public string Rationale { get; private set; } = string.Empty;
    public string? Conditions { get; private set; }
    public UserId ReviewerId { get; private set; }
    public DateTimeOffset DecidedAtUtc { get; private set; }
    public bool IsCurrent { get; private set; }
    public ExamReadinessDecisionId? SupersededByDecisionId { get; private set; }
    public DateTimeOffset? SupersededAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<ExamReadinessDecision> Record(
        ExamReadinessDecisionId id,
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        int version,
        ExamReadinessOutcome outcome,
        ExamReadinessCheckStatus pedagogicalCheck,
        ExamReadinessCheckStatus administrativeCheck,
        ExamReadinessCheckStatus financialCheck,
        ExamReadinessCheckStatus regulatoryCheck,
        string rationale,
        string? conditions,
        UserId reviewerId,
        DateTimeOffset decidedAtUtc)
    {
        if (id.IsEmpty)
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.InvalidIdentifier);
        if (organizationId.IsEmpty)
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.InvalidOrganization);
        if (studentId.IsEmpty)
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.InvalidStudent);
        if (trainingPathId.IsEmpty)
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.InvalidTrainingPath);
        if (reviewerId.IsEmpty)
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.InvalidReviewer);
        if (version <= 0)
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.InvalidVersion);
        if (!Enum.IsDefined(outcome)
            || !Enum.IsDefined(pedagogicalCheck)
            || !Enum.IsDefined(administrativeCheck)
            || !Enum.IsDefined(financialCheck)
            || !Enum.IsDefined(regulatoryCheck))
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.InvalidStatus);
        if (string.IsNullOrWhiteSpace(rationale) || rationale.Trim().Length > 4000)
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.InvalidRationale);
        if (outcome == ExamReadinessOutcome.ReadyWithConditions && string.IsNullOrWhiteSpace(conditions))
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.ConditionsRequired);

        bool hasBlockingCheck = pedagogicalCheck == ExamReadinessCheckStatus.Blocked
            || administrativeCheck == ExamReadinessCheckStatus.Blocked
            || financialCheck == ExamReadinessCheckStatus.Blocked
            || regulatoryCheck == ExamReadinessCheckStatus.Blocked;

        if ((outcome is ExamReadinessOutcome.Ready or ExamReadinessOutcome.ReadyWithConditions) && hasBlockingCheck)
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.ReadyWithBlockingCheck);

        if (outcome == ExamReadinessOutcome.Ready
            && (!IsSatisfied(pedagogicalCheck)
                || !IsSatisfied(administrativeCheck)
                || !IsSatisfied(financialCheck)
                || !IsSatisfied(regulatoryCheck)))
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.ReadyRequiresSatisfiedChecks);
        if (outcome == ExamReadinessOutcome.AdministrativeBlock && administrativeCheck != ExamReadinessCheckStatus.Blocked)
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.AdministrativeBlockRequired);
        if (outcome == ExamReadinessOutcome.FinancialBlock && financialCheck != ExamReadinessCheckStatus.Blocked)
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.FinancialBlockRequired);
        if (outcome == ExamReadinessOutcome.RegulatoryBlock && regulatoryCheck != ExamReadinessCheckStatus.Blocked)
            return Result.Failure<ExamReadinessDecision>(ExamReadinessDecisionErrors.RegulatoryBlockRequired);

        var decision = new ExamReadinessDecision(
            id,
            organizationId,
            studentId,
            trainingPathId,
            version,
            outcome,
            pedagogicalCheck,
            administrativeCheck,
            financialCheck,
            regulatoryCheck,
            rationale.Trim(),
            string.IsNullOrWhiteSpace(conditions) ? null : conditions.Trim(),
            reviewerId,
            decidedAtUtc);

        decision.RaiseDomainEvent(new ExamReadinessDecisionRecordedDomainEvent(
            decision.Id,
            organizationId,
            studentId,
            trainingPathId,
            version,
            outcome,
            reviewerId));

        if (outcome is ExamReadinessOutcome.Ready or ExamReadinessOutcome.ReadyWithConditions)
        {
            decision.RaiseDomainEvent(new StudentMarkedExamReadyDomainEvent(
                decision.Id,
                organizationId,
                studentId,
                trainingPathId,
                reviewerId));
        }

        return Result.Success(decision);
    }

    private static bool IsSatisfied(ExamReadinessCheckStatus status) =>
        status is ExamReadinessCheckStatus.Satisfied or ExamReadinessCheckStatus.NotApplicable;

    public Result Supersede(ExamReadinessDecisionId replacementDecisionId, DateTimeOffset atUtc, UserId actorUserId)
    {
        if (!IsCurrent || SupersededByDecisionId is not null)
            return Result.Failure(ExamReadinessDecisionErrors.AlreadySuperseded);

        IsCurrent = false;
        SupersededByDecisionId = replacementDecisionId;
        SupersededAtUtc = atUtc.ToUniversalTime();
        SetModifiedAudit(atUtc, actorUserId);

        RaiseDomainEvent(new ExamReadinessDecisionSupersededDomainEvent(
            Id,
            replacementDecisionId,
            OrganizationId,
            StudentId));

        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset atUtc, UserId? byUserId)
    {
        if (CreatedAtUtc != default)
            return;

        CreatedAtUtc = atUtc.ToUniversalTime();
        CreatedByUserId = byUserId;
    }

    public void SetModifiedAudit(DateTimeOffset atUtc, UserId? byUserId)
    {
        LastModifiedAtUtc = atUtc.ToUniversalTime();
        LastModifiedByUserId = byUserId;
    }
}
