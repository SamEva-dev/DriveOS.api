using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Terminate;

public sealed record TerminateTrainingContractCommand(
    OrganizationId OrganizationId,
    TrainingContractId ContractId,
    string Reason,
    DateOnly EffectiveDate,
    UserId ActorUserId) : ICommand<TerminateTrainingContractResponse>;

public sealed record TerminateTrainingContractResponse(
    Guid ContractId,
    string Status,
    DateOnly EffectiveDate,
    DateTimeOffset TerminatedAtUtc);
