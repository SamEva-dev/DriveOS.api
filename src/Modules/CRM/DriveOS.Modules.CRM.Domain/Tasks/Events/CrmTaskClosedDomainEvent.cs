using DriveOS.SharedKernel.Domain;

namespace DriveOS.Modules.CRM.Domain.Tasks.Events;

public sealed record CrmTaskClosedDomainEvent(
    CrmTaskId TaskId,
    OrganizationId OrganizationId,
    CrmTaskStatus Status,
    DateTimeOffset ClosedAtUtc
) : DomainEvent;
