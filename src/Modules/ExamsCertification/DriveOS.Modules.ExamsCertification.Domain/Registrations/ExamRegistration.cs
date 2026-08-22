using DriveOS.Modules.ExamsCertification.Domain.Registrations.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations;

/// <summary>
/// Represents the durable registration workflow linking one student to one allocatable examination place.
/// The aggregate stores the readiness decision used at registration time and the exact place/center snapshot,
/// so later provider changes never erase the basis on which the registration was initiated.
/// </summary>
public sealed class ExamRegistration : AggregateRoot<ExamRegistrationId>, IAuditableEntity
{
    private ExamRegistration() { }

    private ExamRegistration(
        ExamRegistrationId id,
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        ExamReadinessDecisionId readinessDecisionId,
        ExamPlaceId examPlaceId,
        ExamCenterId examCenterId,
        string examType,
        string licenseCategory,
        DateTimeOffset scheduledStartUtc,
        DateTimeOffset scheduledEndUtc,
        string providerCode,
        string? externalPlaceId,
        Guid operationId,
        string requestFingerprint,
        UserId createdByUserId,
        DateTimeOffset createdAtUtc) : base(id)
    {
        OrganizationId = organizationId;
        StudentId = studentId;
        TrainingPathId = trainingPathId;
        ReadinessDecisionId = readinessDecisionId;
        ExamPlaceId = examPlaceId;
        ExamCenterId = examCenterId;
        ExamType = examType;
        LicenseCategory = licenseCategory;
        ScheduledStartUtc = scheduledStartUtc.ToUniversalTime();
        ScheduledEndUtc = scheduledEndUtc.ToUniversalTime();
        ProviderCode = providerCode;
        ExternalPlaceId = externalPlaceId;
        OperationId = operationId;
        RequestFingerprint = requestFingerprint;
        Status = ExamRegistrationStatus.PlaceAssigned;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public TrainingPathId TrainingPathId { get; private set; }
    public ExamReadinessDecisionId ReadinessDecisionId { get; private set; }
    public ExamPlaceId ExamPlaceId { get; private set; }
    public ExamCenterId ExamCenterId { get; private set; }
    public string ExamType { get; private set; } = string.Empty;
    public string LicenseCategory { get; private set; } = string.Empty;
    public DateTimeOffset ScheduledStartUtc { get; private set; }
    public DateTimeOffset ScheduledEndUtc { get; private set; }
    public string ProviderCode { get; private set; } = string.Empty;
    public string? ExternalPlaceId { get; private set; }
    public string? ExternalRegistrationId { get; private set; }
    public string? CandidateReference { get; private set; }
    public ExamRegistrationStatus Status { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public bool IsActive => Status is ExamRegistrationStatus.Draft
        or ExamRegistrationStatus.PlaceAssigned
        or ExamRegistrationStatus.PendingSubmission
        or ExamRegistrationStatus.Submitted
        or ExamRegistrationStatus.Confirmed
        or ExamRegistrationStatus.CorrectionRequested;

    public static Result<ExamRegistration> Create(
        ExamRegistrationId id,
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        ExamReadinessDecisionId readinessDecisionId,
        ExamPlaceId examPlaceId,
        ExamCenterId examCenterId,
        string examType,
        string licenseCategory,
        DateTimeOffset scheduledStartUtc,
        DateTimeOffset scheduledEndUtc,
        string providerCode,
        string? externalPlaceId,
        Guid operationId,
        string requestFingerprint,
        UserId createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        if (id.IsEmpty) return Result.Failure<ExamRegistration>(ExamRegistrationErrors.InvalidIdentifier);
        if (organizationId.IsEmpty) return Result.Failure<ExamRegistration>(ExamRegistrationErrors.InvalidOrganization);
        if (studentId.IsEmpty) return Result.Failure<ExamRegistration>(ExamRegistrationErrors.InvalidStudent);
        if (trainingPathId.IsEmpty) return Result.Failure<ExamRegistration>(ExamRegistrationErrors.InvalidTrainingPath);
        if (readinessDecisionId.IsEmpty) return Result.Failure<ExamRegistration>(ExamRegistrationErrors.InvalidReadinessDecision);
        if (examPlaceId.IsEmpty) return Result.Failure<ExamRegistration>(ExamRegistrationErrors.InvalidPlace);
        if (examCenterId.IsEmpty) return Result.Failure<ExamRegistration>(ExamRegistrationErrors.InvalidCenter);
        if (string.IsNullOrWhiteSpace(examType) || string.IsNullOrWhiteSpace(licenseCategory)) return Result.Failure<ExamRegistration>(ExamRegistrationErrors.InvalidExam);
        if (scheduledEndUtc <= scheduledStartUtc) return Result.Failure<ExamRegistration>(ExamRegistrationErrors.InvalidPeriod);
        if (operationId == Guid.Empty || string.IsNullOrWhiteSpace(requestFingerprint)) return Result.Failure<ExamRegistration>(ExamRegistrationErrors.InvalidOperation);

        var registration = new ExamRegistration(
            id, organizationId, studentId, trainingPathId, readinessDecisionId, examPlaceId, examCenterId,
            examType.Trim(), licenseCategory.Trim(), scheduledStartUtc, scheduledEndUtc,
            string.IsNullOrWhiteSpace(providerCode) ? "manual" : providerCode.Trim(),
            string.IsNullOrWhiteSpace(externalPlaceId) ? null : externalPlaceId.Trim(),
            operationId, requestFingerprint.Trim(), createdByUserId, createdAtUtc);

        registration.RaiseDomainEvent(new ExamRegistrationCreatedDomainEvent(id, organizationId, studentId, examPlaceId, readinessDecisionId));
        registration.RaiseDomainEvent(new ExamRegistrationPlaceAssignedDomainEvent(id, organizationId, examPlaceId));
        return Result.Success(registration);
    }


    /// <summary>Updates official candidate data without changing the examination place or readiness snapshot.</summary>
    public Result UpdateCandidateReference(string candidateReference, UserId actor, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(candidateReference))
            return Result.Failure(ExamRegistrationErrors.CandidateReferenceRequired);
        if (Status is not (ExamRegistrationStatus.PlaceAssigned or ExamRegistrationStatus.CorrectionRequested))
            return Result.Failure(ExamRegistrationErrors.OfficialDataLocked);

        CandidateReference = candidateReference.Trim();
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        return Result.Success();
    }


    /// <summary>Moves the local registration into the provider submission workflow.</summary>
    public Result MarkPendingSubmission(UserId actor, DateTimeOffset now)
    {
        if (Status is not (ExamRegistrationStatus.PlaceAssigned or ExamRegistrationStatus.CorrectionRequested or ExamRegistrationStatus.PendingSubmission))
            return Result.Failure(ExamRegistrationErrors.InvalidSubmissionTransition);
        Status = ExamRegistrationStatus.PendingSubmission;
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        return Result.Success();
    }

    public Result MarkSubmitted(string? externalRegistrationId, string? candidateReference, UserId actor, DateTimeOffset now)
    {
        if (Status is not (ExamRegistrationStatus.PlaceAssigned or ExamRegistrationStatus.PendingSubmission or ExamRegistrationStatus.Submitted))
            return Result.Failure(ExamRegistrationErrors.InvalidSubmissionTransition);
        Status = ExamRegistrationStatus.Submitted;
        ExternalRegistrationId = string.IsNullOrWhiteSpace(externalRegistrationId) ? ExternalRegistrationId : externalRegistrationId.Trim();
        CandidateReference = string.IsNullOrWhiteSpace(candidateReference) ? CandidateReference : candidateReference.Trim();
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        return Result.Success();
    }

    public Result MarkConfirmed(string? externalRegistrationId, string? candidateReference, UserId actor, DateTimeOffset now)
    {
        if (Status is not (ExamRegistrationStatus.PendingSubmission or ExamRegistrationStatus.Submitted or ExamRegistrationStatus.Confirmed))
            return Result.Failure(ExamRegistrationErrors.InvalidSubmissionTransition);
        Status = ExamRegistrationStatus.Confirmed;
        ExternalRegistrationId = string.IsNullOrWhiteSpace(externalRegistrationId) ? ExternalRegistrationId : externalRegistrationId.Trim();
        CandidateReference = string.IsNullOrWhiteSpace(candidateReference) ? CandidateReference : candidateReference.Trim();
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        return Result.Success();
    }

    public Result MarkRejected(UserId actor, DateTimeOffset now)
    {
        if (Status is not (ExamRegistrationStatus.PendingSubmission or ExamRegistrationStatus.Submitted))
            return Result.Failure(ExamRegistrationErrors.InvalidSubmissionTransition);
        Status = ExamRegistrationStatus.Rejected;
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        return Result.Success();
    }

    public Result MarkCorrectionRequested(UserId actor, DateTimeOffset now)
    {
        if (Status is not (ExamRegistrationStatus.PendingSubmission or ExamRegistrationStatus.Submitted or ExamRegistrationStatus.Rejected))
            return Result.Failure(ExamRegistrationErrors.InvalidSubmissionTransition);
        Status = ExamRegistrationStatus.CorrectionRequested;
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        return Result.Success();
    }

    public bool MatchesOperation(Guid operationId, string fingerprint) =>
        OperationId == operationId && string.Equals(RequestFingerprint, fingerprint, StringComparison.Ordinal);

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
}
