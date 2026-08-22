using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations.Events;

public sealed record ExamConvocationCreatedDomainEvent(
    ExamConvocationId ConvocationId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    PersonId StudentId) : DomainEvent;

public sealed record ExamConvocationRevisionReceivedDomainEvent(
    ExamConvocationId ConvocationId,
    ExamConvocationRevisionId RevisionId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    int Version,
    DateTimeOffset ScheduledStartUtc) : DomainEvent;

public sealed record ExamConvocationDeliveredDomainEvent(
    ExamConvocationId ConvocationId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    int Version,
    ExamConvocationDeliveryChannel Channel,
    DateTimeOffset DeliveredAtUtc) : DomainEvent;

public sealed record ExamConvocationAcknowledgedDomainEvent(
    ExamConvocationId ConvocationId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    int Version,
    DateTimeOffset AcknowledgedAtUtc) : DomainEvent;
