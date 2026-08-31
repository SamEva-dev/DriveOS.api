using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Compliance;
public sealed record RegisterProfessionalDocumentCommand(ProfessionalDocumentId Id,ProfessionalProfileId ProfileId,Guid DocumentReferenceId,string DocumentTypeCode,string CountryCode,bool Mandatory,DateOnly? IssueDate,DateOnly? ExpirationDate,UserId ActorUserId):ICommand<ProfessionalDocumentId>;
public sealed record SubmitProfessionalDocumentCommand(ProfessionalDocumentId Id,UserId ActorUserId):ICommand;
public sealed record ApproveProfessionalDocumentCommand(ProfessionalDocumentId Id,ProfessionalVerificationMethod Method,UserId ActorUserId):ICommand;
public sealed record RejectProfessionalDocumentCommand(ProfessionalDocumentId Id,string Reason,UserId ActorUserId):ICommand;
public sealed record RegisterProfessionalCredentialCommand(ProfessionalCredentialId Id,ProfessionalProfileId ProfileId,string CredentialTypeCode,string CountryCode,string IssuingAuthority,string? ReferenceNumber,DateOnly ValidFrom,DateOnly? ValidUntil,string[] CategoryCodes,ProfessionalDocumentId? EvidenceDocumentId,UserId ActorUserId):ICommand<ProfessionalCredentialId>;
public sealed record VerifyProfessionalCredentialCommand(ProfessionalCredentialId Id,ProfessionalVerificationMethod Method,UserId ActorUserId):ICommand;
public sealed record RejectProfessionalCredentialCommand(ProfessionalCredentialId Id,string Reason,UserId ActorUserId):ICommand;

public sealed record CreateProfessionalComplianceRequirementCommand(
    ProfessionalComplianceRequirementId Id,
    string RequirementCode,
    string CountryCode,
    ProfessionalType ProfessionalType,
    ProfessionalEvidenceKind EvidenceKind,
    string EvidenceTypeCode,
    bool Mandatory,
    bool Blocking,
    string[] ApplicableCategoryCodes,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    int Version,
    UserId ActorUserId) : ICommand<ProfessionalComplianceRequirementId>;

public sealed record ReevaluateProfessionalComplianceCommand(
    ProfessionalProfileId ProfileId,
    UserId ActorUserId) : ICommand<ProfessionalComplianceEvaluation>;

public sealed record ProfessionalComplianceEvaluation(
    ProfessionalComplianceStatus Status,
    string[] MissingRequirementCodes,
    string[] InvalidRequirementCodes,
    string[] PendingRequirementCodes,
    DateTimeOffset EvaluatedAtUtc);

public sealed record GetProfessionalComplianceQuery(ProfessionalProfileId ProfileId) : IQuery<ProfessionalComplianceResponse>;

public sealed record ProfessionalComplianceDocumentResponse(
    Guid Id,
    Guid DocumentReferenceId,
    string DocumentTypeCode,
    string CountryCode,
    bool Mandatory,
    DateOnly? IssueDate,
    DateOnly? ExpirationDate,
    ProfessionalDocumentStatus Status,
    ProfessionalVerificationMethod? VerificationMethod,
    DateTimeOffset? VerifiedAtUtc,
    Guid? VerifiedByUserId,
    string? RejectionReason,
    Guid? SupersededById);

public sealed record ProfessionalComplianceCredentialResponse(
    Guid Id,
    string CredentialTypeCode,
    string CountryCode,
    string IssuingAuthority,
    string? ReferenceNumber,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    string[] CategoryCodes,
    Guid? EvidenceDocumentId,
    ProfessionalCredentialStatus Status,
    ProfessionalVerificationMethod? VerificationMethod,
    DateTimeOffset? VerifiedAtUtc,
    Guid? VerifiedByUserId,
    string? RejectionReason);

public sealed record ProfessionalComplianceResponse(
    Guid ProfileId,
    ProfessionalComplianceStatus Status,
    ProfessionalComplianceDocumentResponse[] Documents,
    ProfessionalComplianceCredentialResponse[] Credentials);
