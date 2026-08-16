using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Tasks;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Activities.CreateActivity;

public sealed record CreateCrmActivityCommand(
    OrganizationId OrganizationId,
    LeadId? LeadId,
    CrmActivityType Type,
    CrmActivityDirection Direction,
    string Subject,
    string? Details,
    DateTimeOffset OccurredAtUtc,
    UserId? AdvisorUserId,
    CrmActivityMetadata Metadata,
    string? NextActionTitle = null,
    DateTimeOffset? NextActionDueAtUtc = null,
    CrmTaskType NextActionType = CrmTaskType.FollowUp
) : ICommand<Guid>;
