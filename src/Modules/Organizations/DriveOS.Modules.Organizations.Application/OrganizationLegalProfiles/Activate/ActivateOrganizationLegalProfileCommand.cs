using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Activate;

public sealed record ActivateOrganizationLegalProfileCommand(
    OrganizationId OrganizationId,
    int ExpectedRevision
) : ICommand;
