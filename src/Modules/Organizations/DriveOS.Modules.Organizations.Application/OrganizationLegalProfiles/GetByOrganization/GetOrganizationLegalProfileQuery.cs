using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.GetByOrganization;

public sealed record GetOrganizationLegalProfileQuery(OrganizationId OrganizationId)
    : IQuery<OrganizationLegalProfileResponse>;
