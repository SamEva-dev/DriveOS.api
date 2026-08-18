using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Signatories;

public sealed record AddTrainingContractSignatoryCommand(OrganizationId OrganizationId, TrainingContractId ContractId, string Kind, PersonId PersonId, OrganizationId? RepresentedOrganizationId, string DisplayName, int SigningOrder, bool IsRequired, string? AuthorityReference, UserId ActorUserId) : ICommand<Guid>;
public sealed record UpdateTrainingContractSignatoryCommand(OrganizationId OrganizationId, TrainingContractId ContractId, TrainingContractSignatoryId SignatoryId, int SigningOrder, bool IsRequired, string DisplayName, string? AuthorityReference, UserId ActorUserId) : ICommand;
public sealed record RemoveTrainingContractSignatoryCommand(OrganizationId OrganizationId, TrainingContractId ContractId, TrainingContractSignatoryId SignatoryId, UserId ActorUserId) : ICommand;
public sealed record DecideTrainingContractSignatoryAuthorityCommand(OrganizationId OrganizationId, TrainingContractId ContractId, TrainingContractSignatoryId SignatoryId, bool Approved, string? Reason, UserId ActorUserId) : ICommand;
