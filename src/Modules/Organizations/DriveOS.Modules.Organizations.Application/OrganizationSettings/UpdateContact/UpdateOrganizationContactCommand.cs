using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateContact;

public sealed record UpdateOrganizationContactCommand(
    OrganizationId OrganizationId,
    string? Email,
    string? Phone,
    string? Website,
    int ExpectedVersion
) : ICommand;
