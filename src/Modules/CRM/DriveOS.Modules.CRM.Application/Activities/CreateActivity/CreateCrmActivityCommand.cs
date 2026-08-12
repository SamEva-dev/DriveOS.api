using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Activities.CreateActivity;

public sealed record CreateCrmActivityCommand(
    OrganizationId OrganizationId,
    LeadId LeadId,
    CrmActivityType Type,
    CrmActivityDirection Direction,
    string Subject,
    string? Details,
    DateTimeOffset OccurredAtUtc) : ICommand<Guid>;
