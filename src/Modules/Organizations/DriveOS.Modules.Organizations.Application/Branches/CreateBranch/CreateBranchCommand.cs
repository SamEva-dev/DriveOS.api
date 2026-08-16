using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Branches.CreateBranch;

public sealed record CreateBranchCommand(
    OrganizationId OrganizationId,
    string Name,
    string Code,
    BranchType BranchType,
    string AddressLine1,
    string? AddressLine2,
    string PostalCode,
    string City,
    string TimeZoneId,
    bool IsPrimary
) : ICommand<BranchId>;
