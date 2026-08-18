using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Activate;

public sealed record ActivateTrainingContractCommand(
    OrganizationId OrganizationId,
    TrainingContractId ContractId,
    UserId ActorUserId) : ICommand<ActivateTrainingContractResponse>;

public sealed record ActivateTrainingContractResponse(
    Guid ContractId,
    string Status,
    DateTimeOffset ActivatedAtUtc);
