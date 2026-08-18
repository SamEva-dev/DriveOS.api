using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Contracts.Application.Auditing;
using DriveOS.Modules.Contracts.Application.ContractDocuments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Read;

public sealed record TrainingContractHistoryResponse(
    Guid ContractId,
    string ContractNumber,
    string CurrentStatus,
    int CurrentVersionNumber,
    IReadOnlyList<TrainingContractVersionResponse> Versions,
    IReadOnlyList<ContractAmendmentResponse> Amendments,
    IReadOnlyList<ContractDocumentResponse> Documents,
    SignatureProcessResponse? CurrentSignatureProcess,
    IReadOnlyList<ContractAuditEntryResponse> Audit);

public sealed record GetTrainingContractHistoryQuery(
    OrganizationId OrganizationId,
    TrainingContractId ContractId)
    : IQuery<TrainingContractHistoryResponse>;
