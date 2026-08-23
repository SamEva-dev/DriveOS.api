using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations.Events;

public sealed record RegulatoryIntegrationConnectionConfiguredDomainEvent(
    RegulatoryIntegrationConnectionId ConnectionId,
    OrganizationId OrganizationId,
    BranchId? BranchId,
    string CountryCode,
    string ProviderCode) : DomainEvent;
