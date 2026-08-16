using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Documents;

public static class StudentDocumentErrors
{
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.Documents.Owner.Invalid",
        "errors.students.documents.owner.invalid"
    );
    public static readonly Error InvalidMetadata = Error.Validation(
        "Students.Documents.Metadata.Invalid",
        "errors.students.documents.metadata.invalid"
    );
    public static readonly Error NotFound = Error.NotFound(
        "Students.Documents.NotFound",
        "errors.students.documents.notFound"
    );
    public static readonly Error InvalidStatus = Error.Conflict(
        "Students.Documents.Status.Invalid",
        "errors.students.documents.status.invalid"
    );
    public static readonly Error ReasonRequired = Error.Validation(
        "Students.Documents.Reason.Required",
        "errors.students.documents.reason.required"
    );
    public static readonly Error FileRequired = Error.Validation(
        "Students.Documents.File.Required",
        "errors.students.documents.file.required"
    );
    public static readonly Error FileTooLarge = Error.Validation(
        "Students.Documents.File.TooLarge",
        "errors.students.documents.file.tooLarge"
    );
    public static readonly Error FileUnsafe = Error.Validation(
        "Students.Documents.File.Unsafe",
        "errors.students.documents.file.unsafe"
    );
    public static readonly Error VersionNotFound = Error.NotFound(
        "Students.Documents.Version.NotFound",
        "errors.students.documents.version.notFound"
    );
}
