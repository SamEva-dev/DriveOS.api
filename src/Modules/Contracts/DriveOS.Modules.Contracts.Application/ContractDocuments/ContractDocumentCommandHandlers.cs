using System.Security.Cryptography;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Generate;
using DriveOS.Modules.Contracts.Domain.ContractDocuments;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Contracts.Application.ContractDocuments;

internal sealed class UploadContractDocumentCommandHandler(ITrainingContractRepository contracts,IContractDocumentRepository documents,ITrainingContractDocumentStorage storage,IContractsUnitOfWork uow,IClock clock):ICommandHandler<UploadContractDocumentCommand,ContractDocumentResponse>
{
 public async Task<Result<ContractDocumentResponse>> Handle(UploadContractDocumentCommand c,CancellationToken ct)
 {
  var contract=await contracts.GetByIdAsync(c.ContractId,ct); if(contract is null||contract.OrganizationId!=c.OrganizationId)return Result.Failure<ContractDocumentResponse>(TrainingContractErrors.NotFound);
  if(!Enum.TryParse<ContractDocumentType>(c.DocumentType,true,out var type)||!Enum.TryParse<ContractDocumentVisibility>(c.Visibility,true,out var visibility)||c.Content is null||c.Content.Length==0||c.Content.Length>50*1024*1024)return Result.Failure<ContractDocumentResponse>(ContractDocumentErrors.Invalid);
  var id=ContractDocumentId.New(); string sha=Convert.ToHexString(SHA256.HashData(c.Content)); string reference=await storage.StoreAsync(c.OrganizationId,c.ContractId,contract.CurrentVersionNumber,c.FileName,c.Content,ct); var now=clock.UtcNow;
  var vr=ContractDocumentVersion.Create(ContractDocumentVersionId.New(),id,1,c.FileName,c.ContentType,c.Content.LongLength,reference,sha,c.ActorUserId,now); if(vr.IsFailure)return Result.Failure<ContractDocumentResponse>(vr.Error);
  var dr=ContractDocument.Create(id,c.OrganizationId,c.ContractId,contract.CurrentVersionNumber,type,c.Title,visibility,c.RetainUntil,c.RetentionLegalBasis,vr.Value); if(dr.IsFailure)return Result.Failure<ContractDocumentResponse>(dr.Error);
  dr.Value.SetCreatedAudit(now,c.ActorUserId); await documents.AddAsync(dr.Value,ct); await uow.CommitAsync(ct); return Result.Success(Map(dr.Value));
 }
 internal static ContractDocumentResponse Map(ContractDocument d)=>new(d.Id.Value,d.ContractId.Value,d.ContractVersionNumber,d.DocumentType.ToString(),d.Title,d.Visibility.ToString(),d.RetainUntil,d.RetentionLegalBasis,d.Status.ToString(),d.CurrentVersionNumber,d.CreatedAtUtc,d.CreatedByUserId?.Value,d.ArchivedAtUtc,d.ArchivedByUserId?.Value,d.Versions.OrderByDescending(v=>v.VersionNumber).Select(v=>new ContractDocumentVersionResponse(v.Id.Value,v.VersionNumber,v.FileName,v.ContentType,v.Size,v.Sha256,v.UploadedByUserId.Value,v.UploadedAtUtc)).ToArray());
}
internal sealed class AddContractDocumentVersionCommandHandler(ITrainingContractRepository contracts,IContractDocumentRepository documents,ITrainingContractDocumentStorage storage,IContractsUnitOfWork uow,IClock clock):ICommandHandler<AddContractDocumentVersionCommand,ContractDocumentResponse>
{
 public async Task<Result<ContractDocumentResponse>> Handle(AddContractDocumentVersionCommand c,CancellationToken ct){var contract=await contracts.GetByIdAsync(c.ContractId,ct);if(contract is null||contract.OrganizationId!=c.OrganizationId)return Result.Failure<ContractDocumentResponse>(TrainingContractErrors.NotFound);var d=await documents.GetByIdAsync(c.DocumentId,ct);if(d is null||d.OrganizationId!=c.OrganizationId||d.ContractId!=c.ContractId)return Result.Failure<ContractDocumentResponse>(ContractDocumentErrors.NotFound);if(c.Content is null||c.Content.Length==0||c.Content.Length>50*1024*1024)return Result.Failure<ContractDocumentResponse>(ContractDocumentErrors.InvalidFile);int n=d.CurrentVersionNumber+1;string sha=Convert.ToHexString(SHA256.HashData(c.Content));string reference=await storage.StoreAsync(c.OrganizationId,c.ContractId,contract.CurrentVersionNumber,c.FileName,c.Content,ct);var now=clock.UtcNow;var vr=ContractDocumentVersion.Create(ContractDocumentVersionId.New(),d.Id,n,c.FileName,c.ContentType,c.Content.LongLength,reference,sha,c.ActorUserId,now);if(vr.IsFailure)return Result.Failure<ContractDocumentResponse>(vr.Error);var r=d.AddVersion(vr.Value);if(r.IsFailure)return Result.Failure<ContractDocumentResponse>(r.Error);d.SetModifiedAudit(now,c.ActorUserId);await uow.CommitAsync(ct);return Result.Success(UploadContractDocumentCommandHandler.Map(d));}
}
internal sealed class ArchiveContractDocumentCommandHandler(IContractDocumentRepository documents,IContractsUnitOfWork uow,IClock clock):ICommandHandler<ArchiveContractDocumentCommand>
{public async Task<Result> Handle(ArchiveContractDocumentCommand c,CancellationToken ct){var d=await documents.GetByIdAsync(c.DocumentId,ct);if(d is null||d.OrganizationId!=c.OrganizationId||d.ContractId!=c.ContractId)return Result.Failure(ContractDocumentErrors.NotFound);var now=clock.UtcNow;var r=d.Archive(c.ActorUserId,now);if(r.IsFailure)return r;d.SetModifiedAudit(now,c.ActorUserId);await uow.CommitAsync(ct);return Result.Success();}}
internal sealed class GetContractDocumentsQueryHandler(IContractDocumentReadService read):IQueryHandler<GetContractDocumentsQuery,IReadOnlyList<ContractDocumentResponse>>
{public async Task<Result<IReadOnlyList<ContractDocumentResponse>>> Handle(GetContractDocumentsQuery q,CancellationToken ct)=>Result.Success(await read.ListAsync(q.OrganizationId,q.ContractId,ct));}
