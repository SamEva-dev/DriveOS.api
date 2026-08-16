using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Activities.ImportActivity;

public sealed record ImportCrmActivityCommand(
    OrganizationId OrganizationId,
    LeadId? LeadId,
    CrmActivityType Type,
    CrmActivityDirection Direction,
    string Subject,
    string? Details,
    DateTimeOffset OccurredAtUtc,
    UserId? AdvisorUserId,
    string ExternalId,
    string IdempotencyKey,
    CrmActivitySyncStatus SyncStatus,
    string? SyncErrorKey,
    string? Result,
    int? DurationMinutes,
    bool RequiresRegularization,
    string? AttachmentName,
    string? AttachmentReference
) : ICommand<ImportCrmActivityResult>;

public sealed record ImportCrmActivityResult(Guid ActivityId, bool AlreadyImported);
