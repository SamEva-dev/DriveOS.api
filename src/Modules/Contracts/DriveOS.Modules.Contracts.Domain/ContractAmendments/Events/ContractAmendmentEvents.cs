using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Domain.ContractAmendments.Events;

public sealed record ContractAmendmentDraftCreatedDomainEvent(
    ContractAmendmentId AmendmentId,
    TrainingContractId ContractId,
    OrganizationId OrganizationId,
    int AmendmentNumber,
    int BaseContractVersionNumber,
    DateOnly EffectiveDate) : DomainEvent;

public sealed record ContractAmendmentSignedDomainEvent(
    ContractAmendmentId AmendmentId,
    TrainingContractId ContractId,
    string DocumentSha256,
    UserId RecordedByUserId,
    DateTimeOffset SignedAtUtc) : DomainEvent;

public sealed record ContractAmendmentAppliedDomainEvent(
    ContractAmendmentId AmendmentId,
    TrainingContractId ContractId,
    int NewContractVersionNumber,
    UserId AppliedByUserId,
    DateTimeOffset AppliedAtUtc) : DomainEvent;

public sealed record ContractAmendmentCancelledDomainEvent(
    ContractAmendmentId AmendmentId,
    TrainingContractId ContractId,
    string Reason,
    UserId CancelledByUserId,
    DateTimeOffset CancelledAtUtc) : DomainEvent;
