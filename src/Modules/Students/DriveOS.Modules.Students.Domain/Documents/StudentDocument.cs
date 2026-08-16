using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.Students.Domain.Events;

namespace DriveOS.Modules.Students.Domain.Documents;

public sealed class StudentDocument : AggregateRoot<StudentDocumentId>
{
    private readonly List<StudentDocumentVersion> versions = [];
    private readonly List<StudentDocumentAccessLog> accessLogs = [];

    private StudentDocument() { }

    private StudentDocument(
        StudentDocumentId id,
        OrganizationId org,
        PersonId studentId,
        DraftEnrollmentId? enrollmentId,
        string type,
        StudentDocumentCategory category,
        StudentDocumentVisibility visibility,
        DateOnly? expires,
        UserId actor,
        DateTimeOffset now
    )
        : base(id)
    {
        OrganizationId = org;
        StudentId = studentId;
        EnrollmentId = enrollmentId;
        DocumentType = type;
        Category = category;
        Visibility = visibility;
        ExpiresOn = expires;
        Status = StudentDocumentStatus.Requested;
        RequestedAtUtc = now;
        RequestedByUserId = actor;
        RaiseDomainEvent(new StudentAggregateChangedDomainEvent<StudentDocumentId>(Id, StudentId, OrganizationId, "Requested"));
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public DraftEnrollmentId? EnrollmentId { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public StudentDocumentCategory Category { get; private set; }
    public StudentDocumentVisibility Visibility { get; private set; }
    public DateOnly? ExpiresOn { get; private set; }
    public StudentDocumentStatus Status { get; private set; }
    public int CurrentVersion { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public UserId RequestedByUserId { get; private set; }
    public string? DecisionReason { get; private set; }
    public IReadOnlyCollection<StudentDocumentVersion> Versions => versions;
    public IReadOnlyCollection<StudentDocumentAccessLog> AccessLogs => accessLogs;

    public static Result<StudentDocument> Request(
        OrganizationId org,
        PersonId studentId,
        DraftEnrollmentId? enrollmentId,
        string type,
        StudentDocumentCategory category,
        StudentDocumentVisibility visibility,
        DateOnly? expires,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (org.IsEmpty || studentId.IsEmpty || enrollmentId is { IsEmpty: true })
            return Result.Failure<StudentDocument>(StudentDocumentErrors.InvalidOwner);
        if (string.IsNullOrWhiteSpace(type) || visibility == StudentDocumentVisibility.None)
            return Result.Failure<StudentDocument>(StudentDocumentErrors.InvalidMetadata);
        return Result.Success(
            new StudentDocument(
                StudentDocumentId.New(),
                org,
                studentId,
                enrollmentId,
                type.Trim(),
                category,
                visibility,
                expires,
                actor,
                now
            )
        );
    }

    public Result<Guid> AddVersion(
        string fileName,
        string contentType,
        long size,
        string checksum,
        string storageReference,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (Status == StudentDocumentStatus.Archived)
            return Result.Failure<Guid>(StudentDocumentErrors.InvalidStatus);
        if (
            string.IsNullOrWhiteSpace(fileName)
            || string.IsNullOrWhiteSpace(storageReference)
            || size <= 0
        )
            return Result.Failure<Guid>(StudentDocumentErrors.FileRequired);
        foreach (var old in versions.Where(x => x.IsCurrent))
            old.Replace(now);
        CurrentVersion++;
        var item = StudentDocumentVersion.Create(
            Id,
            CurrentVersion,
            fileName,
            contentType,
            size,
            checksum,
            storageReference,
            actor,
            now
        );
        versions.Add(item);
        Status = StudentDocumentStatus.PendingReview;
        DecisionReason = null;
        return Result.Success(item.Id);
    }

    public Result Validate(bool approve, string reason, UserId actor, DateTimeOffset now)
    {
        if (Status != StudentDocumentStatus.PendingReview)
            return Result.Failure(StudentDocumentErrors.InvalidStatus);
        if (!approve && string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StudentDocumentErrors.ReasonRequired);
        Status = approve ? StudentDocumentStatus.Approved : StudentDocumentStatus.Rejected;
        DecisionReason = approve ? null : reason.Trim();
        var current = versions.Single(x => x.IsCurrent);
        current.Review(approve, actor, now);
        return Result.Success();
    }

    public Result Share(StudentDocumentVisibility visibility, UserId actor, DateTimeOffset now)
    {
        if (Status == StudentDocumentStatus.Archived)
            return Result.Failure(StudentDocumentErrors.InvalidStatus);
        if (visibility == StudentDocumentVisibility.None)
            return Result.Failure(StudentDocumentErrors.InvalidMetadata);
        Visibility = visibility;
        Log(StudentDocumentAccessAction.Shared, actor, now);
        return Result.Success();
    }

    public Result Archive(string reason, UserId actor, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StudentDocumentErrors.ReasonRequired);
        Status = StudentDocumentStatus.Archived;
        DecisionReason = reason.Trim();
        return Result.Success();
    }

    public Result LogDownload(Guid versionId, UserId actor, DateTimeOffset now)
    {
        if (!versions.Any(x => x.Id == versionId))
            return Result.Failure(StudentDocumentErrors.VersionNotFound);
        Log(StudentDocumentAccessAction.Downloaded, actor, now, versionId);
        return Result.Success();
    }

    private void Log(
        StudentDocumentAccessAction action,
        UserId actor,
        DateTimeOffset now,
        Guid? version = null
    ) => accessLogs.Add(StudentDocumentAccessLog.Create(Id, version, action, actor, now));
}

public sealed class StudentDocumentVersion
{
    private StudentDocumentVersion() { }

    public Guid Id { get; private set; }
    public StudentDocumentId StudentDocumentId { get; private set; }
    public int VersionNumber { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string Checksum { get; private set; } = string.Empty;
    public string StorageReference { get; private set; } = string.Empty;
    public bool IsCurrent { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
    public UserId UploadedByUserId { get; private set; }
    public DateTimeOffset? ReviewedAtUtc { get; private set; }
    public UserId? ReviewedByUserId { get; private set; }
    public DateTimeOffset? ReplacedAtUtc { get; private set; }

    internal static StudentDocumentVersion Create(
        Guid documentId,
        int number,
        string name,
        string type,
        long size,
        string checksum,
        string reference,
        UserId actor,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            StudentDocumentId = new StudentDocumentId(documentId),
            VersionNumber = number,
            FileName = name.Trim(),
            ContentType = type.Trim(),
            SizeBytes = size,
            Checksum = checksum,
            StorageReference = reference,
            IsCurrent = true,
            UploadedAtUtc = now,
            UploadedByUserId = actor,
        };

    internal void Replace(DateTimeOffset now)
    {
        IsCurrent = false;
        ReplacedAtUtc = now;
    }

    internal void Review(bool approved, UserId actor, DateTimeOffset now)
    {
        ReviewedAtUtc = now;
        ReviewedByUserId = actor;
    }
}

public sealed class StudentDocumentAccessLog
{
    private StudentDocumentAccessLog() { }

    public Guid Id { get; private set; }
    public StudentDocumentId StudentDocumentId { get; private set; }
    public Guid? VersionId { get; private set; }
    public StudentDocumentAccessAction Action { get; private set; }
    public UserId ActorUserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }

    internal static StudentDocumentAccessLog Create(
        Guid documentId,
        Guid? version,
        StudentDocumentAccessAction action,
        UserId actor,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            StudentDocumentId =new StudentDocumentId( documentId),
            VersionId = version,
            Action = action,
            ActorUserId = actor,
            OccurredAtUtc = now,
        };
}
