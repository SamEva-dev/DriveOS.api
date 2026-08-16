using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Branches.UpdateBranch;

public sealed record UpdateBranchCommand(
    OrganizationId OrganizationId,
    BranchId BranchId,
    string Name,
    BranchType BranchType,
    string AddressLine1,
    string? AddressLine2,
    string PostalCode,
    string City,
    string TimeZoneId
) : ICommand;
