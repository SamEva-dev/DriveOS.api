using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Leads.GetLead;

public sealed record GetLeadQuery(
    OrganizationId OrganizationId,
    LeadId LeadId) : IQuery<LeadResponse>;
