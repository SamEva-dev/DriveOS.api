using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Contracts.Domain.ContractDocuments;
public sealed class ContractDocumentVersion : Entity<ContractDocumentVersionId>
{
    private ContractDocumentVersion() { }
    private ContractDocumentVersion(ContractDocumentVersionId id, ContractDocumentId documentId, int versionNumber, string fileName, string contentType, long size, string storageReference, string sha256, UserId uploadedByUserId, DateTimeOffset uploadedAtUtc) : base(id)
    { DocumentId=documentId; VersionNumber=versionNumber; FileName=fileName; ContentType=contentType; Size=size; StorageReference=storageReference; Sha256=sha256; UploadedByUserId=uploadedByUserId; UploadedAtUtc=uploadedAtUtc; }
    public ContractDocumentId DocumentId { get; private set; }
    public int VersionNumber { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long Size { get; private set; }
    public string StorageReference { get; private set; } = string.Empty;
    public string Sha256 { get; private set; } = string.Empty;
    public UserId UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
    public static Result<ContractDocumentVersion> Create(ContractDocumentVersionId id, ContractDocumentId documentId, int versionNumber, string fileName, string contentType, long size, string storageReference, string sha256, UserId actor, DateTimeOffset at)
    {
        if(id.IsEmpty || documentId.IsEmpty || versionNumber<1 || string.IsNullOrWhiteSpace(fileName) || fileName.Trim().Length>255 || string.IsNullOrWhiteSpace(contentType) || contentType.Trim().Length>120 || size<=0 || size>50*1024*1024 || string.IsNullOrWhiteSpace(storageReference) || sha256?.Trim().Length!=64 || actor.IsEmpty || at==default)
            return Result.Failure<ContractDocumentVersion>(ContractDocumentErrors.InvalidFile);
        return Result.Success(new ContractDocumentVersion(id,documentId,versionNumber,fileName.Trim(),contentType.Trim(),size,storageReference.Trim(),sha256.Trim().ToUpperInvariant(),actor,at.ToUniversalTime()));
    }
}
