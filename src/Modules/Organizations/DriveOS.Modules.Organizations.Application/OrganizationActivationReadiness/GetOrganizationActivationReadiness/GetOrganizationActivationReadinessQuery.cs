using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.GetOrganizationActivationReadiness;

public sealed record GetOrganizationActivationReadinessQuery(OrganizationId OrganizationId)
    : IQuery<OrganizationActivationReadinessReport>;
