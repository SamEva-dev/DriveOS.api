using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Documents;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Documents;

public sealed record StudentDocumentListResponse(
    Guid StudentId,
    IReadOnlyList<StudentDocumentItem> Items
);

public sealed record StudentDocumentItem(
    Guid Id,
    Guid? EnrollmentId,
    string DocumentType,
    StudentDocumentCategory Category,
    StudentDocumentStatus Status,
    int CurrentVersion,
    DateTimeOffset? UploadedAtUtc,
    DateOnly? ExpiresOn,
    StudentDocumentVisibility Visibility,
    string? DecisionReason
);

public sealed record StudentDocumentDownload(
    Stream Content,
    string FileName,
    string ContentType,
    long Length
);

public sealed record GetStudentDocumentsQuery(
    OrganizationId OrganizationId,
    PersonId StudentId,
    DraftEnrollmentId? EnrollmentId
) : IQuery<StudentDocumentListResponse>;

public sealed record RequestStudentDocumentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    DraftEnrollmentId? EnrollmentId,
    string DocumentType,
    StudentDocumentCategory Category,
    StudentDocumentVisibility Visibility,
    DateOnly? ExpiresOn,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record UploadStudentDocumentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid DocumentId,
    string FileName,
    string ContentType,
    long Length,
    Stream Content,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record ValidateStudentDocumentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid DocumentId,
    bool Approve,
    string? Reason,
    UserId ActorUserId
) : ICommand;

public sealed record ShareStudentDocumentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid DocumentId,
    StudentDocumentVisibility Visibility,
    UserId ActorUserId
) : ICommand;

public sealed record ArchiveStudentDocumentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid DocumentId,
    string Reason,
    UserId ActorUserId
) : ICommand;

public sealed record DownloadStudentDocumentQuery(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid DocumentId,
    int? Version,
    UserId ActorUserId
) : IQuery<StudentDocumentDownload>;

public interface IStudentDocumentService
{
    Task<StudentDocumentListResponse?> GetAsync(
        GetStudentDocumentsQuery query,
        CancellationToken ct = default
    );
    Task<Result<Guid>> RequestAsync(
        RequestStudentDocumentCommand command,
        CancellationToken ct = default
    );
    Task<Result<Guid>> UploadAsync(
        UploadStudentDocumentCommand command,
        CancellationToken ct = default
    );
    Task<Result> ValidateAsync(
        ValidateStudentDocumentCommand command,
        CancellationToken ct = default
    );
    Task<Result> ShareAsync(ShareStudentDocumentCommand command, CancellationToken ct = default);
    Task<Result> ArchiveAsync(
        ArchiveStudentDocumentCommand command,
        CancellationToken ct = default
    );
    Task<Result<StudentDocumentDownload>> DownloadAsync(
        DownloadStudentDocumentQuery query,
        CancellationToken ct = default
    );
}

public interface IStudentDocumentStorage
{
    Task<string> StoreAsync(
        OrganizationId organizationId,
        Guid documentId,
        Guid versionId,
        Stream content,
        CancellationToken ct
    );
    Task<Stream?> OpenReadAsync(string storageReference, CancellationToken ct);
}

public interface IStudentDocumentSecurityScanner
{
    Task<bool> IsSafeAsync(
        string fileName,
        string contentType,
        ReadOnlyMemory<byte> content,
        CancellationToken ct
    );
}

public static class StudentDocumentApplicationErrors
{
    public static readonly Error StudentNotFound = Error.NotFound(
        "Students.Documents.Student.NotFound",
        "errors.students.documents.student.notFound"
    );
    public static readonly Error EnrollmentNotFound = Error.NotFound(
        "Students.Documents.Enrollment.NotFound",
        "errors.students.documents.enrollment.notFound"
    );
    public static readonly Error FileUnavailable = Error.NotFound(
        "Students.Documents.File.Unavailable",
        "errors.students.documents.file.unavailable"
    );
}
