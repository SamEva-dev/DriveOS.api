using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;

public enum ProfessionalContractSignatureOrder{Sequential=1,Parallel=2}

public sealed record ProfessionalContractSignatoryInput(
    PersonId PersonId,
    string Role,
    int SigningOrder,
    bool IsRequired);

public sealed record CreateProfessionalServiceContractCommand(
    ProfessionalServiceContractId Id,
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId,
    string ContractNumber,
    string ContractType,
    ProfessionalContractSignatureOrder SignatureOrder,
    ProfessionalContractSignatoryInput[] Signatories,
    UserId ActorUserId):ICommand<ProfessionalServiceContractSnapshot>;

public sealed record GenerateProfessionalServiceContractCommand(
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId,
    string DocumentReference,
    string DocumentSha256,
    UserId ActorUserId):ICommand<ProfessionalServiceContractSnapshot>;

public sealed record ReviseProfessionalServiceContractCommand(
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId,
    string DocumentReference,
    string DocumentSha256,
    string Reason,
    UserId ActorUserId):ICommand<ProfessionalServiceContractSnapshot>;

public sealed record SendProfessionalServiceContractForSignatureCommand(
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId,
    UserId ActorUserId):ICommand<ProfessionalServiceContractSnapshot>;

public sealed record RecordProfessionalServiceContractSignatureCommand(
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId,
    PersonId SignatoryPersonId,
    string DocumentSha256,
    string SignatureMethod,
    string AuthenticationMethod,
    string Provider,
    string ProviderReference,
    string? CertificateReference,
    string? IpAddress,
    DateTimeOffset SignedAtUtc,
    UserId ActorUserId):ICommand<ProfessionalServiceContractSnapshot>;

public sealed record TerminateProfessionalServiceContractCommand(
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId,
    string Reason,
    UserId ActorUserId):ICommand;

public sealed record PrepareProfessionalEngagementContractCommand(
    ProfessionalEngagementId Id,
    OrganizationId OrganizationId,
    UserId ActorUserId):ICommand<ProfessionalServiceContractSnapshot>;

public sealed record PrepareProfessionalEngagementComplianceCommand(
    ProfessionalEngagementId Id,
    OrganizationId OrganizationId,
    UserId ActorUserId):ICommand;

public sealed record ProfessionalServiceContractCreationRequest(
    ProfessionalServiceContractId Id,
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId,
    ProfessionalProfileId ProfessionalProfileId,
    Guid ProviderOrganizationId,
    string ContractNumber,
    string ContractType,
    ProfessionalContractSignatureOrder SignatureOrder,
    string TermsSnapshotJson,
    ProfessionalContractSignatoryInput[] Signatories,
    UserId ActorUserId);

public sealed record ProfessionalServiceContractSignatureRequest(
    ProfessionalEngagementId EngagementId,
    PersonId SignatoryPersonId,
    string DocumentSha256,
    string SignatureMethod,
    string AuthenticationMethod,
    string Provider,
    string ProviderReference,
    string? CertificateReference,
    string? IpAddress,
    DateTimeOffset SignedAtUtc,
    UserId ActorUserId);

public sealed record ProfessionalServiceContractSignatorySnapshot(
    Guid PersonId,
    string Role,
    int SigningOrder,
    bool IsRequired,
    DateTimeOffset? SignedAtUtc,
    DateTimeOffset? ReceivedAtUtc,
    string? SignatureMethod,
    string? AuthenticationMethod,
    string? Provider,
    string? ProviderReference,
    string? CertificateReference);

public sealed record ProfessionalServiceContractVersionSnapshotView(
    int Version,
    string? DocumentReference,
    string? DocumentSha256,
    string Status,
    DateTimeOffset? GeneratedAtUtc,
    DateTimeOffset? SentForSignatureAtUtc,
    DateTimeOffset? SignedAtUtc,
    string RevisionReason,
    DateTimeOffset SupersededAtUtc,
    Guid SupersededByUserId);

public sealed record ProfessionalServiceContractSnapshot(
    Guid ContractId,
    Guid EngagementId,
    string ContractNumber,
    string ContractType,
    int Version,
    string Status,
    string SignatureOrder,
    string? DocumentReference,
    string? DocumentSha256,
    DateTimeOffset? GeneratedAtUtc,
    DateTimeOffset? SentForSignatureAtUtc,
    DateTimeOffset? SignedAtUtc,
    DateTimeOffset? TerminatedAtUtc,
    string? TerminationReason,
    int RequiredSignatories,
    int SignedRequiredSignatories,
    ProfessionalServiceContractSignatorySnapshot[] Signatories,
    ProfessionalServiceContractVersionSnapshotView[] PreviousVersions);

/// <summary>
/// BC-13 anti-corruption port. Contract lifecycle and signature evidence are owned by Contracts.
/// </summary>
public interface IProfessionalServiceContractGateway
{
    Task<Result<ProfessionalServiceContractSnapshot>> CreateAsync(
        ProfessionalServiceContractCreationRequest request,
        CancellationToken cancellationToken=default);

    Task<ProfessionalServiceContractSnapshot?> GetByEngagementAsync(
        ProfessionalEngagementId engagementId,
        CancellationToken cancellationToken=default);

    Task<Result<ProfessionalServiceContractSnapshot>> GenerateAsync(
        ProfessionalEngagementId engagementId,
        string documentReference,
        string documentSha256,
        UserId actorUserId,
        CancellationToken cancellationToken=default);

    Task<Result<ProfessionalServiceContractSnapshot>> CreateRevisionAsync(
        ProfessionalEngagementId engagementId,
        string documentReference,
        string documentSha256,
        string reason,
        UserId actorUserId,
        CancellationToken cancellationToken=default);

    Task<Result<ProfessionalServiceContractSnapshot>> SendForSignatureAsync(
        ProfessionalEngagementId engagementId,
        UserId actorUserId,
        CancellationToken cancellationToken=default);

    Task<Result<ProfessionalServiceContractSnapshot>> RecordSignatureAsync(
        ProfessionalServiceContractSignatureRequest request,
        CancellationToken cancellationToken=default);

    Task<Result> TerminateAsync(
        ProfessionalEngagementId engagementId,
        string reason,
        UserId actorUserId,
        CancellationToken cancellationToken=default);
}
