using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Tasks;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Tasks.CreateTask;

public sealed record CreateCrmTaskCommand(OrganizationId OrganizationId, LeadId LeadId,
    CrmTaskType Type, string Title, string? Notes, DateTimeOffset DueAtUtc,
    UserId? AssignedToUserId) : ICommand<Guid>;
