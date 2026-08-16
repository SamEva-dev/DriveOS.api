using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Leads.ManageLifecycle;

public sealed record CloseLeadCommand(
    OrganizationId OrganizationId,
    LeadId LeadId,
    LeadStatus Decision,
    LeadClosureReason Reason,
    string? Comment
) : ICommand;

public sealed record SetLeadDormantCommand(
    OrganizationId OrganizationId,
    LeadId LeadId,
    LeadClosureReason Reason,
    DateTimeOffset ResumeAtUtc,
    UserId ResponsibleUserId,
    string? CampaignCode,
    string? Comment
) : ICommand;

public sealed record ReferLeadToPartnerCommand(
    OrganizationId OrganizationId,
    LeadId LeadId,
    string PartnerName,
    string SharedDataDescription,
    DateTimeOffset ConsentCollectedAtUtc,
    string? Comment
) : ICommand;

public sealed record ReopenLeadCommand(
    OrganizationId OrganizationId,
    LeadId LeadId,
    string? Comment
) : ICommand;
