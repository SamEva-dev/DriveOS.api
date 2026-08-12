using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Leads.QualifyLead;

public sealed record QualifyLeadCommand(OrganizationId OrganizationId, LeadId LeadId,
    string Need, string LicenseCategory, string Availability, DateOnly? TargetDate,
    FinancingOption Financing, string? Notes) : ICommand;
