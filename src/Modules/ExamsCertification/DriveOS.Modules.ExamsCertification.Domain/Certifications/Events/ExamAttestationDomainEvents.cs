using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Certifications.Events;

public sealed record ExamAttestationIssuedDomainEvent(ExamAttestationId AttestationId, OrganizationId OrganizationId,
    ExamResultId ExamResultId, int ResultRevision, ExamAttestationType Type, DocumentId DocumentId) : DomainEvent;

public sealed record ExamAttestationDocumentCorrectedDomainEvent(ExamAttestationId AttestationId, OrganizationId OrganizationId,
    int PreviousVersion, int CurrentVersion, DocumentId DocumentId) : DomainEvent;

public sealed record ExamAttestationSignedDomainEvent(ExamAttestationId AttestationId, OrganizationId OrganizationId,
    int Version, string SignatureProcessReference) : DomainEvent;

public sealed record ExamAttestationDeliveredDomainEvent(ExamAttestationId AttestationId, OrganizationId OrganizationId,
    int Version, ExamAttestationDeliveryChannel Channel) : DomainEvent;

public sealed record ExamAttestationRevokedDomainEvent(ExamAttestationId AttestationId, OrganizationId OrganizationId,
    ExamResultId ExamResultId, string ReasonCode) : DomainEvent;

public sealed record ExamAttestationSupersededDomainEvent(ExamAttestationId AttestationId, OrganizationId OrganizationId,
    ExamResultId ExamResultId, int ResultRevision) : DomainEvent;
