using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Contracts.Domain.ContractDocuments.Events;
public sealed record ContractDocumentCreatedDomainEvent(ContractDocumentId DocumentId, TrainingContractId ContractId, OrganizationId OrganizationId, ContractDocumentType Type, int VersionNumber) : DomainEvent;
public sealed record ContractDocumentVersionAddedDomainEvent(ContractDocumentId DocumentId, TrainingContractId ContractId, int VersionNumber) : DomainEvent;
public sealed record ContractDocumentArchivedDomainEvent(ContractDocumentId DocumentId, TrainingContractId ContractId, UserId ArchivedByUserId, DateTimeOffset ArchivedAtUtc) : DomainEvent;
