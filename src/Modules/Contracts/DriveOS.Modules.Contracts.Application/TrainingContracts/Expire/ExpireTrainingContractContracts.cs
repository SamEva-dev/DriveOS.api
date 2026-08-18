using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Expire;

public sealed record ExpireTrainingContractCommand(OrganizationId OrganizationId, TrainingContractId ContractId, UserId ActorUserId)
    : ICommand<ExpireTrainingContractResponse>;

public sealed record ExpireTrainingContractResponse(Guid ContractId, string Status, DateOnly EffectiveDate, DateTimeOffset ExpiredAtUtc);
