using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Domain.ContractAmendments;

public interface IContractAmendmentRepository
{
    Task<ContractAmendment?> GetByIdAsync(ContractAmendmentId amendmentId, CancellationToken cancellationToken = default);
    Task<int> GetNextNumberAsync(OrganizationId organizationId, TrainingContractId contractId, CancellationToken cancellationToken = default);
    Task AddAsync(ContractAmendment amendment, CancellationToken cancellationToken = default);
}
