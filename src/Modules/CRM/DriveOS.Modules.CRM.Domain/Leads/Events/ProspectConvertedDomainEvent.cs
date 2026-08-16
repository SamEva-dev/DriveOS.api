using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Leads.Events;

public sealed record ProspectConvertedDomainEvent(
    LeadId LeadId,
    OrganizationId OrganizationId,
    PersonId PersonId,
    DraftEnrollmentId DraftEnrollmentId,
    DateTimeOffset ConvertedAtUtc
) : DomainEvent;
