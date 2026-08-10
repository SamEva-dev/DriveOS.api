using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Leads.ChangeLeadStatus;

public sealed record ChangeLeadStatusCommand(
    OrganizationId OrganizationId,
    LeadId LeadId,
    LeadStatus TargetStatus,
    string? Reason) : ICommand;
