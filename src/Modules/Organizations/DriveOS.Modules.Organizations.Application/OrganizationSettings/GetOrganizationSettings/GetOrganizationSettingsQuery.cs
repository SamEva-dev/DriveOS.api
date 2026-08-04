using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.GetOrganizationSettings;

public sealed record GetOrganizationSettingsQuery(
    OrganizationId OrganizationId)
    : IQuery<OrganizationSettingsResponse>;
