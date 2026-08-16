using DriveOS.SharedKernel.Domain;

namespace DriveOS.Modules.CRM.Domain.Tasks.Events;

public sealed record CrmTaskCreatedDomainEvent(
    CrmTaskId TaskId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    DateTimeOffset DueAtUtc
) : DomainEvent;
