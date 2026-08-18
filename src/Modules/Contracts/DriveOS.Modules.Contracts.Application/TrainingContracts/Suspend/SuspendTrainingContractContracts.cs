using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Suspend;

public sealed record SuspendTrainingContractCommand(
    OrganizationId OrganizationId,
    TrainingContractId ContractId,
    string Reason,
    DateOnly EffectiveDate,
    DateOnly? ExpectedResumeDate,
    UserId ActorUserId) : ICommand<SuspendTrainingContractResponse>;

public sealed record SuspendTrainingContractResponse(
    Guid ContractId,
    string Status,
    DateOnly EffectiveDate,
    DateOnly? ExpectedResumeDate,
    DateTimeOffset SuspendedAtUtc);
