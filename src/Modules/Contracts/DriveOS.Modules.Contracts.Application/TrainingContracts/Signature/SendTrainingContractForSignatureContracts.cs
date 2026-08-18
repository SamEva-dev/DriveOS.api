using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Signature;

public sealed record SendTrainingContractForSignatureCommand(OrganizationId OrganizationId, TrainingContractId ContractId, UserId ActorUserId) : ICommand<SendTrainingContractForSignatureResponse>;
public sealed record SendTrainingContractForSignatureResponse(Guid SignatureProcessId, string Status, DateTimeOffset RequestedAtUtc);
