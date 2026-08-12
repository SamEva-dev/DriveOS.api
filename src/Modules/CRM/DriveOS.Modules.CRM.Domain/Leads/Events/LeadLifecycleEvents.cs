using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Leads.Events;

public sealed record LeadMarkedLostDomainEvent(
    LeadId LeadId, OrganizationId OrganizationId, LeadStatus Decision,
    LeadClosureReason Reason, string? Comment) : DomainEvent;

public sealed record LeadMarkedDormantDomainEvent(
    LeadId LeadId, OrganizationId OrganizationId, LeadClosureReason Reason,
    DateTimeOffset ResumeAtUtc, UserId ResponsibleUserId) : DomainEvent;

public sealed record LeadReopenedDomainEvent(
    LeadId LeadId, OrganizationId OrganizationId, LeadStatus PreviousStatus,
    string? Comment) : DomainEvent;

public sealed record LeadReferredToPartnerDomainEvent(
    LeadId LeadId, OrganizationId OrganizationId, string PartnerName,
    string SharedDataDescription, DateTimeOffset ConsentCollectedAtUtc) : DomainEvent;
