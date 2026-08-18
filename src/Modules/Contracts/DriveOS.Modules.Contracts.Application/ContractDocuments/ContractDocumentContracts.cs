using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Contracts.Application.ContractDocuments;
public sealed record ContractDocumentVersionResponse(Guid Id,int VersionNumber,string FileName,string ContentType,long Size,string Sha256,Guid UploadedByUserId,DateTimeOffset UploadedAtUtc);
public sealed record ContractDocumentResponse(Guid Id,Guid ContractId,int ContractVersionNumber,string DocumentType,string Title,string Visibility,DateOnly? RetainUntil,string? RetentionLegalBasis,string Status,int CurrentVersionNumber,DateTimeOffset CreatedAtUtc,Guid? CreatedByUserId,DateTimeOffset? ArchivedAtUtc,Guid? ArchivedByUserId,IReadOnlyList<ContractDocumentVersionResponse> Versions);
public sealed record UploadContractDocumentCommand(OrganizationId OrganizationId,TrainingContractId ContractId,string DocumentType,string Title,string Visibility,DateOnly? RetainUntil,string? RetentionLegalBasis,string FileName,string ContentType,byte[] Content,UserId ActorUserId):ICommand<ContractDocumentResponse>;
public sealed record AddContractDocumentVersionCommand(OrganizationId OrganizationId,TrainingContractId ContractId,ContractDocumentId DocumentId,string FileName,string ContentType,byte[] Content,UserId ActorUserId):ICommand<ContractDocumentResponse>;
public sealed record ArchiveContractDocumentCommand(OrganizationId OrganizationId,TrainingContractId ContractId,ContractDocumentId DocumentId,UserId ActorUserId):ICommand;
public sealed record GetContractDocumentsQuery(OrganizationId OrganizationId,TrainingContractId ContractId):IQuery<IReadOnlyList<ContractDocumentResponse>>;
public interface IContractDocumentReadService { Task<IReadOnlyList<ContractDocumentResponse>> ListAsync(OrganizationId organizationId,TrainingContractId contractId,CancellationToken cancellationToken=default); }
