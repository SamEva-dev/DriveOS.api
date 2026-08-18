using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Domain.TrainingContracts;

public interface ITrainingContractRepository
{
    Task<TrainingContract?> GetByIdAsync(
        TrainingContractId contractId,
        CancellationToken cancellationToken = default);

    Task<TrainingContract?> GetByContractNumberAsync(
        OrganizationId organizationId,
        string contractNumber,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TrainingContract contract,
        CancellationToken cancellationToken = default);
}
