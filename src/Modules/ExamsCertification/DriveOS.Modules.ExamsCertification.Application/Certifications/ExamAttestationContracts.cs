using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Certifications;

public sealed record ExamAttestationRevisionResponse(Guid Id, int Version, string TemplateCode, int TemplateVersion,
    Guid DocumentId, string DocumentSha256, bool HasPublicVerification, string? SignatureProcessReference,
    string? SignatureEvidenceHash, Guid? SignedByUserId, DateTimeOffset? SignedAtUtc, Guid GeneratedByUserId, DateTimeOffset GeneratedAtUtc);

public sealed record ExamAttestationResponse(Guid Id, Guid ExamResultId, int ResultRevision, Guid ExamAttemptId,
    Guid ExamRegistrationId, Guid StudentId, int AttemptNumber, string Type, string Reference, int CurrentVersion,
    Guid? SupersedesAttestationId, string Status, DateTimeOffset IssuedAtUtc, Guid IssuedByUserId, DateTimeOffset? ExpiresAtUtc,
    DateTimeOffset? DeliveredAtUtc, Guid? DeliveredByUserId, string? DeliveryChannel, DateTimeOffset? RevokedAtUtc,
    Guid? RevokedByUserId, string? RevocationReasonCode, string? RevocationNotes, DateTimeOffset? SupersededAtUtc,
    IReadOnlyList<ExamAttestationRevisionResponse> Revisions);

public sealed record PublicExamAttestationVerificationResponse(Guid AttestationId, string Type, string Reference,
    string Status, int Version, DateTimeOffset IssuedAtUtc, DateTimeOffset? ExpiresAtUtc, bool IsValid, bool IsSigned);

public sealed record IssueExamAttestationCommand(OrganizationId OrganizationId, ExamResultId ResultId, string Type,
    string Reference, string TemplateCode, int TemplateVersion, DocumentId DocumentId, string DocumentSha256,
    string? PublicVerificationToken, DateTimeOffset? ExpiresAtUtc, ExamAttestationId? SupersedesAttestationId,
    Guid OperationId, UserId ActorUserId) : ICommand<ExamAttestationResponse>;
public sealed record CorrectExamAttestationDocumentCommand(OrganizationId OrganizationId, ExamAttestationId AttestationId,
    string TemplateCode, int TemplateVersion, DocumentId DocumentId, string DocumentSha256, string? PublicVerificationToken,
    UserId ActorUserId) : ICommand<ExamAttestationResponse>;
public sealed record SignExamAttestationCommand(OrganizationId OrganizationId, ExamAttestationId AttestationId,
    string SignatureProcessReference, string SignatureEvidenceHash, UserId ActorUserId) : ICommand<ExamAttestationResponse>;
public sealed record DeliverExamAttestationCommand(OrganizationId OrganizationId, ExamAttestationId AttestationId,
    string DeliveryChannel, UserId ActorUserId) : ICommand<ExamAttestationResponse>;
public sealed record RevokeExamAttestationCommand(OrganizationId OrganizationId, ExamAttestationId AttestationId,
    string ReasonCode, string? Notes, UserId ActorUserId) : ICommand<ExamAttestationResponse>;
public sealed record GetExamAttestationQuery(OrganizationId OrganizationId, ExamAttestationId AttestationId) : IQuery<ExamAttestationResponse>;
public sealed record GetExamResultAttestationsQuery(OrganizationId OrganizationId, ExamResultId ResultId) : IQuery<IReadOnlyList<ExamAttestationResponse>>;
public sealed record GetStudentExamAttestationsQuery(OrganizationId OrganizationId, PersonId StudentId) : IQuery<IReadOnlyList<ExamAttestationResponse>>;
public sealed record VerifyExamAttestationQuery(string PublicToken) : IQuery<PublicExamAttestationVerificationResponse>;
