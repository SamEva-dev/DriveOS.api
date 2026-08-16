using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Conversions.Events;

public sealed record LeadConversionCompletedDomainEvent(
    LeadConversionId ConversionId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    PersonId StudentPersonId,
    DraftEnrollmentId StudentEnrollmentId,
    DateTimeOffset CompletedAtUtc
) : DomainEvent;
