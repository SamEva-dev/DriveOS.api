using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateProfile;

public sealed record UpdateOrganizationProfileCommand(
    OrganizationId OrganizationId,
    string? TradeName,
    string? RegistrationNumber,
    string? TaxNumber,
    int ExpectedVersion
) : ICommand;
