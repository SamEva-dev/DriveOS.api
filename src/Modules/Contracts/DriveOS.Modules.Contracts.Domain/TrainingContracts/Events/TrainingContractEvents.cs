using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Domain.TrainingContracts.Events;

public sealed record TrainingContractDraftCreatedDomainEvent(
    TrainingContractId ContractId,
    OrganizationId OrganizationId,
    BranchId BranchId,
    PersonId StudentId,
    CommercialOfferId SourceOfferId,
    int SourceOfferVersion,
    string ContractNumber,
    decimal TotalAmount,
    string Currency
) : DomainEvent;

public sealed record TrainingContractVersionCreatedDomainEvent(
    TrainingContractId ContractId,
    TrainingContractVersionId VersionId,
    int VersionNumber,
    CommercialOfferId SourceOfferId,
    int SourceOfferVersion,
    string RevisionReason
) : DomainEvent;


public sealed record TrainingContractGeneratedDomainEvent(
    TrainingContractId ContractId,
    int VersionNumber,
    string DocumentSha256,
    DateTimeOffset GeneratedAtUtc
) : DomainEvent;

public sealed record TrainingContractSignatoryAddedDomainEvent(
    TrainingContractId ContractId,
    TrainingContractSignatoryId SignatoryId,
    PersonId PersonId,
    string Kind,
    int SigningOrder,
    bool IsRequired) : DomainEvent;

public sealed record TrainingContractSignatoryRemovedDomainEvent(
    TrainingContractId ContractId,
    TrainingContractSignatoryId SignatoryId) : DomainEvent;

public sealed record TrainingContractSignatoryAuthorityDecidedDomainEvent(
    TrainingContractId ContractId,
    TrainingContractSignatoryId SignatoryId,
    bool Approved,
    UserId ActorUserId,
    DateTimeOffset DecidedAtUtc) : DomainEvent;

public sealed record TrainingContractSentForSignatureDomainEvent(
    TrainingContractId ContractId,
    SignatureProcessId SignatureProcessId,
    int ContractVersionNumber,
    UserId RequestedByUserId,
    DateTimeOffset RequestedAtUtc) : DomainEvent;

public sealed record TrainingContractSignatorySignedDomainEvent(
    TrainingContractId ContractId,
    TrainingContractSignatoryId SignatoryId,
    SignatureEvidenceId EvidenceId,
    UserId ActorUserId,
    DateTimeOffset SignedAtUtc) : DomainEvent;

public sealed record TrainingContractSignedDomainEvent(
    TrainingContractId ContractId,
    int ContractVersionNumber,
    UserId ActorUserId,
    DateTimeOffset SignedAtUtc) : DomainEvent;

public sealed record TrainingContractActivatedDomainEvent(
    TrainingContractId ContractId,
    int ContractVersionNumber,
    UserId ActorUserId,
    DateTimeOffset ActivatedAtUtc) : DomainEvent;

public sealed record TrainingContractAmendedDomainEvent(
    TrainingContractId ContractId,
    ContractAmendmentId AmendmentId,
    int AmendmentNumber,
    int NewVersionNumber,
    UserId AppliedByUserId,
    DateTimeOffset AppliedAtUtc) : DomainEvent;

public sealed record TrainingContractSuspendedDomainEvent(
    TrainingContractId ContractId,
    int ContractVersionNumber,
    DateOnly EffectiveDate,
    DateOnly? ExpectedResumeDate,
    string Reason,
    UserId ActorUserId,
    DateTimeOffset SuspendedAtUtc) : DomainEvent;

public sealed record TrainingContractTerminatedDomainEvent(
    TrainingContractId ContractId,
    int ContractVersionNumber,
    DateOnly EffectiveDate,
    string Reason,
    UserId ActorUserId,
    DateTimeOffset TerminatedAtUtc) : DomainEvent;

public sealed record TrainingContractCompletedDomainEvent(
    TrainingContractId ContractId,
    int ContractVersionNumber,
    DateOnly EffectiveDate,
    string Note,
    UserId ActorUserId,
    DateTimeOffset CompletedAtUtc) : DomainEvent;

public sealed record TrainingContractExpiredDomainEvent(
    TrainingContractId ContractId,
    int ContractVersionNumber,
    DateOnly EffectiveDate,
    UserId ActorUserId,
    DateTimeOffset ExpiredAtUtc) : DomainEvent;
