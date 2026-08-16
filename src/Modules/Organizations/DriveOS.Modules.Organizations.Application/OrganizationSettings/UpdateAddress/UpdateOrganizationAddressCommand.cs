using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateAddress;

public sealed record UpdateOrganizationAddressCommand(
    OrganizationId OrganizationId,
    string? AddressLine1,
    string? AddressLine2,
    string? PostalCode,
    string? City,
    string? Region,
    string AddressCountryCode,
    int ExpectedVersion
) : ICommand;
