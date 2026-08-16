using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Tasks.GetLeadTasks;

public sealed record GetLeadTasksQuery(OrganizationId OrganizationId, LeadId LeadId)
    : IQuery<IReadOnlyList<CrmTaskResponse>>;

public sealed record GetPendingTasksQuery(OrganizationId OrganizationId)
    : IQuery<IReadOnlyList<CrmTaskResponse>>;

public sealed record CrmTaskResponse(
    Guid Id,
    Guid LeadId,
    string Type,
    string Title,
    string? Notes,
    DateTimeOffset DueAtUtc,
    Guid? AssignedToUserId,
    string Status,
    DateTimeOffset? ClosedAtUtc,
    DateTimeOffset CreatedAtUtc
);
