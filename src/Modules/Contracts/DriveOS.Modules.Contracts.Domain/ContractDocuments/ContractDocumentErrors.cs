using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Contracts.Domain.ContractDocuments;
public static class ContractDocumentErrors
{
    public static readonly Error NotFound = Error.NotFound("Contracts.Document.NotFound", "errors.contracts.document.notFound");
    public static readonly Error Invalid = Error.Validation("Contracts.Document.Invalid", "errors.contracts.document.invalid");
    public static readonly Error InvalidFile = Error.Validation("Contracts.Document.File.Invalid", "errors.contracts.document.file.invalid");
    public static readonly Error Archived = Error.Conflict("Contracts.Document.Archived", "errors.contracts.document.archived");
    public static readonly Error AlreadyArchived = Error.Conflict("Contracts.Document.AlreadyArchived", "errors.contracts.document.alreadyArchived");
}
