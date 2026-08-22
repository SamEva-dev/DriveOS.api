using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Submissions.Events;

public sealed record ExamRegistrationSubmissionCreatedDomainEvent(
    ExamRegistrationSubmissionId SubmissionId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    int FileVersion,
    string ProviderCode) : DomainEvent;

public sealed record ExamRegistrationSubmittedDomainEvent(
    ExamRegistrationSubmissionId SubmissionId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    string ProviderCode,
    string? ExternalSubmissionId) : DomainEvent;

public sealed record ExamRegistrationOfficiallyAcceptedDomainEvent(
    ExamRegistrationSubmissionId SubmissionId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    string? ExternalRegistrationId) : DomainEvent;

public sealed record ExamRegistrationOfficiallyRejectedDomainEvent(
    ExamRegistrationSubmissionId SubmissionId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    string ErrorCode) : DomainEvent;

public sealed record ExamRegistrationCorrectionRequestedDomainEvent(
    ExamRegistrationSubmissionId SubmissionId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    string ErrorCode) : DomainEvent;
