using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Complete;

public sealed record CompleteTrainingContractCommand(
    OrganizationId OrganizationId, TrainingContractId ContractId, string Note, DateOnly EffectiveDate, UserId ActorUserId)
    : ICommand<CompleteTrainingContractResponse>;

public sealed record CompleteTrainingContractResponse(Guid ContractId, string Status, DateOnly EffectiveDate, DateTimeOffset CompletedAtUtc);
