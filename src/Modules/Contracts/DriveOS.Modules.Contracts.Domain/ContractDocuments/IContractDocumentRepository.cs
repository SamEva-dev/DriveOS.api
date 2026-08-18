using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Contracts.Domain.ContractDocuments;
public interface IContractDocumentRepository
{
    Task<ContractDocument?> GetByIdAsync(ContractDocumentId id, CancellationToken cancellationToken=default);
    Task AddAsync(ContractDocument document, CancellationToken cancellationToken=default);
}
