using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Domain.SignatureProcesses.Events;

public sealed record ContractSignatureRecordedDomainEvent(
    SignatureProcessId SignatureProcessId,
    TrainingContractId ContractId,
    TrainingContractSignatoryId SignatoryId,
    SignatureEvidenceId EvidenceId,
    DateTimeOffset SignedAtUtc) : DomainEvent;

public sealed record ContractSignatureProcessCompletedDomainEvent(
    SignatureProcessId SignatureProcessId,
    TrainingContractId ContractId,
    int ContractVersionNumber,
    DateTimeOffset CompletedAtUtc) : DomainEvent;
