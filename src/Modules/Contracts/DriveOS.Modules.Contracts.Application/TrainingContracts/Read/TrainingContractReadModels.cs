using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Read;

public sealed record TrainingContractListItemResponse(
    Guid Id,
    string ContractNumber,
    Guid StudentId,
    Guid BranchId,
    int CurrentVersionNumber,
    string Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal TotalAmount,
    string Currency,
    string TrainingCode,
    DateTimeOffset CreatedAtUtc);

public sealed record TrainingContractPartyResponse(
    string Kind,
    Guid? PersonId,
    Guid? OrganizationId,
    string DisplayName,
    string? LegalReference);


public sealed record TrainingContractSignatoryResponse(
    Guid Id,
    string Kind,
    Guid PersonId,
    Guid? RepresentedOrganizationId,
    string DisplayName,
    int SigningOrder,
    bool IsRequired,
    string? AuthorityReference,
    string AuthorityStatus,
    Guid? AuthorityVerifiedByUserId,
    DateTimeOffset? AuthorityVerifiedAtUtc,
    string? AuthorityRejectionReason,
    string Status);


public sealed record SignatureEvidenceResponse(
    Guid Id,
    Guid SignatoryId,
    Guid PersonId,
    string DocumentSha256,
    string SignatureMethod,
    string AuthenticationMethod,
    string Provider,
    string ProviderSignatureReference,
    string? CertificateReference,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset SignedAtUtc,
    DateTimeOffset ReceivedAtUtc,
    Guid RecordedByUserId);

public sealed record SignatureProcessRecipientResponse(
    Guid SignatoryId,
    string Kind,
    Guid PersonId,
    Guid? RepresentedOrganizationId,
    string DisplayName,
    int SigningOrder,
    bool IsRequired,
    bool HasSigned);

public sealed record SignatureProcessResponse(
    Guid Id,
    int ContractVersionNumber,
    string DocumentSha256,
    string SignatureOrder,
    string Status,
    DateTimeOffset RequestedAtUtc,
    Guid RequestedByUserId,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyList<SignatureProcessRecipientResponse> Recipients,
    IReadOnlyList<SignatureEvidenceResponse> Evidence);

public sealed record TrainingContractTermsResponse(
    string TrainingCode,
    decimal PracticalHours,
    string ServicesSnapshot,
    string PaymentScheduleSnapshot,
    string CancellationTerms,
    string BookingRules,
    string StudentObligations,
    string ProviderObligations,
    string ExamPresentationTerms,
    string DataProcessingTerms);

public sealed record TrainingContractVersionResponse(
    Guid Id,
    int VersionNumber,
    Guid SourceOfferId,
    int SourceOfferVersion,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal TotalAmount,
    string Currency,
    string? RevisionReason,
    Guid? CreatedByUserId,
    DateTimeOffset CreatedAtUtc);


public sealed record ContractAmendmentResponse(
    Guid Id,
    int AmendmentNumber,
    int BaseContractVersionNumber,
    string Reason,
    DateOnly EffectiveDate,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal TotalAmount,
    string Currency,
    string Status,
    string? SignedDocumentReference,
    string? SignedDocumentSha256,
    DateTimeOffset? SignedAtUtc,
    DateTimeOffset? AppliedAtUtc,
    string? CancellationReason,
    DateTimeOffset CreatedAtUtc);

public sealed record TrainingContractDetailResponse(
    Guid Id,
    Guid OrganizationId,
    Guid BranchId,
    Guid StudentId,
    Guid SourceOfferId,
    int SourceOfferVersion,
    string ContractNumber,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal TotalAmount,
    string Currency,
    int CurrentVersionNumber,
    string Status,
    TrainingContractTermsResponse Terms,
    IReadOnlyList<TrainingContractPartyResponse> Parties,
    IReadOnlyList<TrainingContractVersionResponse> Versions,
    IReadOnlyList<TrainingContractSignatoryResponse> Signatories,
    IReadOnlyList<ContractAmendmentResponse> Amendments,
    SignatureProcessResponse? CurrentSignatureProcess,
    string? GeneratedDocumentFileName,
    string? GeneratedDocumentContentType,
    string? GeneratedDocumentSha256,
    int? GeneratedDocumentVersionNumber,
    DateTimeOffset? GeneratedAtUtc,
    Guid? GeneratedByUserId,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset? LastModifiedAtUtc,
    Guid? LastModifiedByUserId,
    DateTimeOffset? ActivatedAtUtc,
    Guid? ActivatedByUserId,
    string? SuspensionReason,
    DateOnly? SuspensionEffectiveDate,
    DateOnly? SuspensionExpectedResumeDate,
    DateTimeOffset? SuspendedAtUtc,
    Guid? SuspendedByUserId,
    string? TerminationReason,
    DateOnly? TerminationEffectiveDate,
    DateTimeOffset? TerminatedAtUtc,
    Guid? TerminatedByUserId,
    string? CompletionNote,
    DateOnly? CompletionEffectiveDate,
    DateTimeOffset? CompletedAtUtc,
    Guid? CompletedByUserId,
    DateOnly? ExpirationEffectiveDate,
    DateTimeOffset? ExpiredAtUtc,
    Guid? ExpiredByUserId);

public sealed record GetTrainingContractQuery(
    OrganizationId OrganizationId,
    TrainingContractId ContractId) : IQuery<TrainingContractDetailResponse>;

public sealed record GetTrainingContractsQuery(
    OrganizationId OrganizationId,
    PersonId? StudentId) : IQuery<IReadOnlyList<TrainingContractListItemResponse>>;

public interface ITrainingContractReadService
{
    Task<TrainingContractDetailResponse?> GetAsync(
        OrganizationId organizationId,
        TrainingContractId contractId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrainingContractListItemResponse>> ListAsync(
        OrganizationId organizationId,
        PersonId? studentId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TrainingContractListItemResponse>> SearchAsync(
        SearchTrainingContractsQuery query,
        CancellationToken cancellationToken = default);
}
