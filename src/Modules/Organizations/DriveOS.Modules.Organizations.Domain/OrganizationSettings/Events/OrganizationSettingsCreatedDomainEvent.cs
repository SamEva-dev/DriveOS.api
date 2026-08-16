using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSettings.Events;

public sealed record OrganizationSettingsCreatedDomainEvent(
    OrganizationSettingsId SettingsId,
    OrganizationId OrganizationId,
    int Version
) : DomainEvent;
