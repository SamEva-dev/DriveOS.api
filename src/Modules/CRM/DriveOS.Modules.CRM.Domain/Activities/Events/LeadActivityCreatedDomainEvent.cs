using DriveOS.SharedKernel.Domain;

namespace DriveOS.Modules.CRM.Domain.Activities.Events;

public sealed record LeadActivityCreatedDomainEvent(
    CrmActivityId ActivityId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    CrmActivityType Type,
    DateTimeOffset ActivityOccurredAtUtc
) : DomainEvent;
