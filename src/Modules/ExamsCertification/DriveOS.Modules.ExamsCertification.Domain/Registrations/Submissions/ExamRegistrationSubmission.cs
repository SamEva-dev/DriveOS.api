using DriveOS.Modules.ExamsCertification.Domain.Registrations.Submissions.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Submissions;

/// <summary>
/// Stores one immutable official submission of an examination registration dossier revision.
/// A correction/resubmission creates another aggregate instance, preserving the exact payload and provider response
/// that belonged to every attempt. Raw provider data is retained for audit while UI-facing errors use stable DriveOS keys.
/// </summary>
public sealed class ExamRegistrationSubmission : AggregateRoot<ExamRegistrationSubmissionId>, IAuditableEntity
{
    private ExamRegistrationSubmission() { }

    private ExamRegistrationSubmission(
        ExamRegistrationSubmissionId id,
        OrganizationId organizationId,
        ExamRegistrationId registrationId,
        ExamRegistrationFileId registrationFileId,
        Guid fileRevisionId,
        int fileVersion,
        int submissionVersion,
        string providerCode,
        string payloadJson,
        Guid operationId,
        string requestFingerprint,
        UserId actor,
        DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId;
        RegistrationId = registrationId;
        RegistrationFileId = registrationFileId;
        FileRevisionId = fileRevisionId;
        FileVersion = fileVersion;
        SubmissionVersion = submissionVersion;
        ProviderCode = providerCode.Trim();
        PayloadJson = payloadJson;
        OperationId = operationId;
        RequestFingerprint = requestFingerprint;
        Status = ExamRegistrationSubmissionStatus.Pending;
        CreatedByUserId = actor;
        CreatedAtUtc = now.ToUniversalTime();
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamRegistrationId RegistrationId { get; private set; }
    public ExamRegistrationFileId RegistrationFileId { get; private set; }
    public Guid FileRevisionId { get; private set; }
    public int FileVersion { get; private set; }
    public int SubmissionVersion { get; private set; }
    public string ProviderCode { get; private set; } = string.Empty;
    public string PayloadJson { get; private set; } = string.Empty;
    public ExamRegistrationSubmissionStatus Status { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public string? ExternalSubmissionId { get; private set; }
    public string? ExternalRegistrationId { get; private set; }
    public string? CandidateReference { get; private set; }
    public string? ProviderResponseCode { get; private set; }
    public string? ProviderResponseJson { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessageKey { get; private set; }
    public DateTimeOffset? SubmittedAtUtc { get; private set; }
    public DateTimeOffset? RespondedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public bool IsFinal => Status is ExamRegistrationSubmissionStatus.Accepted
        or ExamRegistrationSubmissionStatus.Rejected
        or ExamRegistrationSubmissionStatus.CorrectionRequested
        or ExamRegistrationSubmissionStatus.Cancelled;

    public static Result<ExamRegistrationSubmission> Create(
        OrganizationId organizationId,
        ExamRegistrationId registrationId,
        ExamRegistrationFileId registrationFileId,
        Guid fileRevisionId,
        int fileVersion,
        int submissionVersion,
        string providerCode,
        string payloadJson,
        Guid operationId,
        string requestFingerprint,
        UserId actor,
        DateTimeOffset now)
    {
        if (organizationId.IsEmpty || registrationId.IsEmpty)
            return Result.Failure<ExamRegistrationSubmission>(ExamRegistrationSubmissionErrors.InvalidRegistration);
        if (registrationFileId.IsEmpty || fileRevisionId == Guid.Empty || fileVersion <= 0 || submissionVersion <= 0)
            return Result.Failure<ExamRegistrationSubmission>(ExamRegistrationSubmissionErrors.InvalidFileRevision);
        if (string.IsNullOrWhiteSpace(providerCode))
            return Result.Failure<ExamRegistrationSubmission>(ExamRegistrationSubmissionErrors.InvalidProvider);
        if (operationId == Guid.Empty || string.IsNullOrWhiteSpace(requestFingerprint) || string.IsNullOrWhiteSpace(payloadJson))
            return Result.Failure<ExamRegistrationSubmission>(ExamRegistrationSubmissionErrors.InvalidOperation);

        var submission = new ExamRegistrationSubmission(
            ExamRegistrationSubmissionId.New(), organizationId, registrationId, registrationFileId,
            fileRevisionId, fileVersion, submissionVersion, providerCode, payloadJson,
            operationId, requestFingerprint.Trim(), actor, now);
        submission.RaiseDomainEvent(new ExamRegistrationSubmissionCreatedDomainEvent(
            submission.Id, organizationId, registrationId, fileVersion, submission.ProviderCode));
        return Result.Success(submission);
    }

    public bool MatchesOperation(Guid operationId, string fingerprint) =>
        OperationId == operationId && string.Equals(RequestFingerprint, fingerprint, StringComparison.Ordinal);

    public Result MarkAwaitingManualSubmission(UserId actor, DateTimeOffset now)
    {
        if (IsFinal) return Result.Failure(ExamRegistrationSubmissionErrors.AlreadyFinalized);
        Status = ExamRegistrationSubmissionStatus.AwaitingManualSubmission;
        Touch(actor, now);
        return Result.Success();
    }

    public Result MarkSubmitted(string? externalSubmissionId, string? providerResponseCode, string? providerResponseJson,
        UserId actor, DateTimeOffset now)
    {
        if (IsFinal) return Result.Failure(ExamRegistrationSubmissionErrors.AlreadyFinalized);
        Status = ExamRegistrationSubmissionStatus.Submitted;
        ExternalSubmissionId = Normalize(externalSubmissionId);
        ProviderResponseCode = Normalize(providerResponseCode);
        ProviderResponseJson = Normalize(providerResponseJson);
        ErrorCode = null;
        ErrorMessageKey = null;
        SubmittedAtUtc ??= now.ToUniversalTime();
        Touch(actor, now);
        RaiseDomainEvent(new ExamRegistrationSubmittedDomainEvent(Id, OrganizationId, RegistrationId, ProviderCode, ExternalSubmissionId));
        return Result.Success();
    }

    public Result MarkAccepted(string? externalSubmissionId, string? externalRegistrationId, string? candidateReference,
        string? providerResponseCode, string? providerResponseJson, UserId actor, DateTimeOffset now)
    {
        if (IsFinal) return Result.Failure(ExamRegistrationSubmissionErrors.AlreadyFinalized);
        Status = ExamRegistrationSubmissionStatus.Accepted;
        ExternalSubmissionId = Normalize(externalSubmissionId) ?? ExternalSubmissionId;
        ExternalRegistrationId = Normalize(externalRegistrationId);
        CandidateReference = Normalize(candidateReference);
        ProviderResponseCode = Normalize(providerResponseCode);
        ProviderResponseJson = Normalize(providerResponseJson);
        ErrorCode = null;
        ErrorMessageKey = null;
        SubmittedAtUtc ??= now.ToUniversalTime();
        RespondedAtUtc = now.ToUniversalTime();
        Touch(actor, now);
        RaiseDomainEvent(new ExamRegistrationOfficiallyAcceptedDomainEvent(Id, OrganizationId, RegistrationId, ExternalRegistrationId));
        return Result.Success();
    }

    public Result MarkRejected(string errorCode, string errorMessageKey, string? providerResponseCode,
        string? providerResponseJson, UserId actor, DateTimeOffset now)
    {
        if (IsFinal) return Result.Failure(ExamRegistrationSubmissionErrors.AlreadyFinalized);
        if (string.IsNullOrWhiteSpace(errorCode) || string.IsNullOrWhiteSpace(errorMessageKey))
            return Result.Failure(ExamRegistrationSubmissionErrors.ProviderRejected);
        Status = ExamRegistrationSubmissionStatus.Rejected;
        ErrorCode = errorCode.Trim();
        ErrorMessageKey = errorMessageKey.Trim();
        ProviderResponseCode = Normalize(providerResponseCode);
        ProviderResponseJson = Normalize(providerResponseJson);
        SubmittedAtUtc ??= now.ToUniversalTime();
        RespondedAtUtc = now.ToUniversalTime();
        Touch(actor, now);
        RaiseDomainEvent(new ExamRegistrationOfficiallyRejectedDomainEvent(Id, OrganizationId, RegistrationId, ErrorCode));
        return Result.Success();
    }

    public Result MarkCorrectionRequested(string errorCode, string errorMessageKey, string? providerResponseCode,
        string? providerResponseJson, UserId actor, DateTimeOffset now)
    {
        if (IsFinal) return Result.Failure(ExamRegistrationSubmissionErrors.AlreadyFinalized);
        if (string.IsNullOrWhiteSpace(errorCode) || string.IsNullOrWhiteSpace(errorMessageKey))
            return Result.Failure(ExamRegistrationSubmissionErrors.ProviderRejected);
        Status = ExamRegistrationSubmissionStatus.CorrectionRequested;
        ErrorCode = errorCode.Trim();
        ErrorMessageKey = errorMessageKey.Trim();
        ProviderResponseCode = Normalize(providerResponseCode);
        ProviderResponseJson = Normalize(providerResponseJson);
        SubmittedAtUtc ??= now.ToUniversalTime();
        RespondedAtUtc = now.ToUniversalTime();
        Touch(actor, now);
        RaiseDomainEvent(new ExamRegistrationCorrectionRequestedDomainEvent(Id, OrganizationId, RegistrationId, ErrorCode));
        return Result.Success();
    }

    public Result MarkFailed(string errorCode, string errorMessageKey, string? providerResponseJson, UserId actor, DateTimeOffset now)
    {
        if (IsFinal) return Result.Failure(ExamRegistrationSubmissionErrors.AlreadyFinalized);
        Status = ExamRegistrationSubmissionStatus.Failed;
        ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? "Exams.RegistrationSubmission.ProviderUnavailable" : errorCode.Trim();
        ErrorMessageKey = string.IsNullOrWhiteSpace(errorMessageKey) ? "errors.exams.registrationSubmission.providerUnavailable" : errorMessageKey.Trim();
        ProviderResponseJson = Normalize(providerResponseJson);
        Touch(actor, now);
        return Result.Success();
    }

    private void Touch(UserId actor, DateTimeOffset now)
    {
        LastModifiedByUserId = actor;
        LastModifiedAtUtc = now.ToUniversalTime();
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
