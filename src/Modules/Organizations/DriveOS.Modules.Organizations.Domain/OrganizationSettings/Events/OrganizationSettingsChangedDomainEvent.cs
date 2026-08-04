using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSettings.Events;

public sealed record OrganizationSettingsChangedDomainEvent(
    OrganizationSettingsId SettingsId,
    OrganizationId OrganizationId,
    OrganizationSettingsSection Section,
    int Version)
    : DomainEvent;
