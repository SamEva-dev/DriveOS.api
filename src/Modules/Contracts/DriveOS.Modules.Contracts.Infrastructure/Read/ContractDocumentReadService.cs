using DriveOS.Modules.Contracts.Application.ContractDocuments;
using DriveOS.Modules.Contracts.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Contracts.Infrastructure.Read;
internal sealed class ContractDocumentReadService(ContractsDbContext db):IContractDocumentReadService
{
 public async Task<IReadOnlyList<ContractDocumentResponse>> ListAsync(OrganizationId org,TrainingContractId contractId,CancellationToken ct=default)
 {
  var docs=await db.ContractDocuments.AsNoTracking().Include(x=>x.Versions).Where(x=>x.OrganizationId==org&&x.ContractId==contractId).OrderByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);
  return docs.Select(d=>new ContractDocumentResponse(d.Id.Value,d.ContractId.Value,d.ContractVersionNumber,d.DocumentType.ToString(),d.Title,d.Visibility.ToString(),d.RetainUntil,d.RetentionLegalBasis,d.Status.ToString(),d.CurrentVersionNumber,d.CreatedAtUtc,d.CreatedByUserId?.Value,d.ArchivedAtUtc,d.ArchivedByUserId?.Value,d.Versions.OrderByDescending(v=>v.VersionNumber).Select(v=>new ContractDocumentVersionResponse(v.Id.Value,v.VersionNumber,v.FileName,v.ContentType,v.Size,v.Sha256,v.UploadedByUserId.Value,v.UploadedAtUtc)).ToArray())).ToArray();
 }
}
