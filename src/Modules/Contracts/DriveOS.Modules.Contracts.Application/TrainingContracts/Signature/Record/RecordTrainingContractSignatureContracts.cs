using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Signature.Record;

public sealed record RecordTrainingContractSignatureCommand(
    OrganizationId OrganizationId,
    TrainingContractId ContractId,
    SignatureProcessId SignatureProcessId,
    TrainingContractSignatoryId SignatoryId,
    string DocumentSha256,
    string SignatureMethod,
    string AuthenticationMethod,
    string Provider,
    string ProviderSignatureReference,
    string? CertificateReference,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset SignedAtUtc,
    UserId ActorUserId) : ICommand<RecordTrainingContractSignatureResponse>;

public sealed record RecordTrainingContractSignatureResponse(
    Guid EvidenceId,
    Guid SignatoryId,
    string ProcessStatus,
    string ContractStatus,
    DateTimeOffset SignedAtUtc);
