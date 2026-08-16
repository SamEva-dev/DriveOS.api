using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Activate;

public sealed record ActivateOrganizationRepresentativeCommand(
    OrganizationId OrganizationId,
    OrganizationRepresentativeId RepresentativeId,
    int ExpectedRevision
) : ICommand;
