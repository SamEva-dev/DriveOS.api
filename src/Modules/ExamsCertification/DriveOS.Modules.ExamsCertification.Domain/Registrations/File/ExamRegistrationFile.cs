using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.File;

/// <summary>
/// Owns the auditable examination registration dossier. Each refresh creates an immutable revision
/// containing the exact prerequisite snapshot used at that moment. Provider submissions can later
/// reference one revision without rewriting earlier dossier states.
/// </summary>
public sealed class ExamRegistrationFile : AggregateRoot<ExamRegistrationFileId>, IAuditableEntity
{
    private readonly List<ExamRegistrationFileRevision> _revisions = [];
    private ExamRegistrationFile() { }

    private ExamRegistrationFile(ExamRegistrationFileId id, OrganizationId organizationId, ExamRegistrationId registrationId,
        PersonId studentId, UserId actor, DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId;
        RegistrationId = registrationId;
        StudentId = studentId;
        Status = ExamRegistrationFileStatus.Incomplete;
        CreatedByUserId = actor;
        CreatedAtUtc = now.ToUniversalTime();
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamRegistrationId RegistrationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public ExamRegistrationFileStatus Status { get; private set; }
    public int CurrentVersion { get; private set; }
    public DateTimeOffset? LastEvaluatedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }
    public IReadOnlyCollection<ExamRegistrationFileRevision> Revisions => _revisions;
    public ExamRegistrationFileRevision? CurrentRevision => _revisions.OrderByDescending(x => x.Version).FirstOrDefault();

    public static Result<ExamRegistrationFile> Create(OrganizationId organizationId, ExamRegistrationId registrationId,
        PersonId studentId, UserId actor, DateTimeOffset now)
    {
        if (organizationId.IsEmpty || registrationId.IsEmpty || studentId.IsEmpty)
            return Result.Failure<ExamRegistrationFile>(ExamRegistrationFileErrors.InvalidRegistration);
        return Result.Success(new ExamRegistrationFile(ExamRegistrationFileId.New(), organizationId, registrationId, studentId, actor, now));
    }

    public Result MarkSubmitted(int fileVersion, UserId actor, DateTimeOffset now)
    {
        if (CurrentVersion != fileVersion || Status != ExamRegistrationFileStatus.Ready)
            return Result.Failure(ExamRegistrationFileErrors.NotReady);
        Status = ExamRegistrationFileStatus.Submitted;
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        return Result.Success();
    }

    public Result MarkOfficiallyAccepted(UserId actor, DateTimeOffset now)
    {
        if (Status is not (ExamRegistrationFileStatus.Submitted or ExamRegistrationFileStatus.OfficiallyAccepted))
            return Result.Failure(ExamRegistrationFileErrors.InvalidSubmissionTransition);
        Status = ExamRegistrationFileStatus.OfficiallyAccepted;
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        return Result.Success();
    }

    public Result MarkOfficiallyRejected(UserId actor, DateTimeOffset now)
    {
        if (Status != ExamRegistrationFileStatus.Submitted)
            return Result.Failure(ExamRegistrationFileErrors.InvalidSubmissionTransition);
        Status = ExamRegistrationFileStatus.OfficiallyRejected;
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        return Result.Success();
    }

    public Result MarkCorrectionRequested(UserId actor, DateTimeOffset now)
    {
        if (Status is not (ExamRegistrationFileStatus.Submitted or ExamRegistrationFileStatus.OfficiallyRejected))
            return Result.Failure(ExamRegistrationFileErrors.InvalidSubmissionTransition);
        Status = ExamRegistrationFileStatus.CorrectionRequested;
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        return Result.Success();
    }

    public Result<ExamRegistrationFileRevision> Refresh(
        IReadOnlyCollection<ExamRegistrationChecklistSnapshotItem> items,
        string? candidateReference,
        string? officialDataJson,
        UserId actor,
        DateTimeOffset now)
    {
        if (Status is not (ExamRegistrationFileStatus.Incomplete or ExamRegistrationFileStatus.Ready or ExamRegistrationFileStatus.CorrectionRequested))
            return Result.Failure<ExamRegistrationFileRevision>(ExamRegistrationFileErrors.RevisionLocked);
        if (items.Count == 0 || items.Any(x => string.IsNullOrWhiteSpace(x.Code)))
            return Result.Failure<ExamRegistrationFileRevision>(ExamRegistrationFileErrors.InvalidSnapshot);

        int nextVersion = CurrentVersion + 1;
        var revision = ExamRegistrationFileRevision.Create(
            Id, nextVersion, items, candidateReference, officialDataJson, actor, now);
        _revisions.Add(revision);
        CurrentVersion = nextVersion;
        LastEvaluatedAtUtc = now.ToUniversalTime();
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;

        bool ready = items.Where(x => x.Required).All(x => x.Status is ExamRegistrationRequirementStatus.Compliant or ExamRegistrationRequirementStatus.NotApplicable);
        Status = ready ? ExamRegistrationFileStatus.Ready : ExamRegistrationFileStatus.Incomplete;
        return Result.Success(revision);
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
}

public sealed class ExamRegistrationFileRevision
{
    private ExamRegistrationFileRevision() { }
    private readonly List<ExamRegistrationChecklistItemSnapshot> _checklist = [];

    public Guid Id { get; private set; }
    public ExamRegistrationFileId RegistrationFileId { get; private set; }
    public int Version { get; private set; }
    public string? CandidateReference { get; private set; }
    public string? OfficialDataJson { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public IReadOnlyCollection<ExamRegistrationChecklistItemSnapshot> Checklist => _checklist;

    internal static ExamRegistrationFileRevision Create(ExamRegistrationFileId fileId, int version,
        IReadOnlyCollection<ExamRegistrationChecklistSnapshotItem> items, string? candidateReference,
        string? officialDataJson, UserId actor, DateTimeOffset now)
    {
        var revision = new ExamRegistrationFileRevision
        {
            Id = Guid.NewGuid(), RegistrationFileId = fileId, Version = version,
            CandidateReference = string.IsNullOrWhiteSpace(candidateReference) ? null : candidateReference.Trim(),
            OfficialDataJson = string.IsNullOrWhiteSpace(officialDataJson) ? null : officialDataJson,
            CreatedAtUtc = now.ToUniversalTime(), CreatedByUserId = actor
        };
        foreach (ExamRegistrationChecklistSnapshotItem item in items)
            revision._checklist.Add(ExamRegistrationChecklistItemSnapshot.Create(revision.Id, item));
        return revision;
    }
}

public sealed class ExamRegistrationChecklistItemSnapshot
{
    private ExamRegistrationChecklistItemSnapshot() { }
    public Guid Id { get; private set; }
    public Guid RevisionId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public bool Required { get; private set; }
    public ExamRegistrationRequirementStatus Status { get; private set; }
    public string MessageKey { get; private set; } = string.Empty;
    public string? Source { get; private set; }
    public string? Evidence { get; private set; }

    internal static ExamRegistrationChecklistItemSnapshot Create(Guid revisionId, ExamRegistrationChecklistSnapshotItem item) => new()
    {
        Id = Guid.NewGuid(), RevisionId = revisionId, Code = item.Code.Trim(), Required = item.Required,
        Status = item.Status, MessageKey = item.MessageKey.Trim(), Source = item.Source, Evidence = item.Evidence
    };
}

public sealed record ExamRegistrationChecklistSnapshotItem(
    string Code,
    bool Required,
    ExamRegistrationRequirementStatus Status,
    string MessageKey,
    string? Source = null,
    string? Evidence = null);
