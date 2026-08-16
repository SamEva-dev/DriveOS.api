using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.GetVersions;

public sealed record GetBranchConfigurationOverrideVersionsQuery(
    OrganizationId OrganizationId,
    BranchId BranchId
) : IQuery<IReadOnlyList<BranchConfigurationOverrideListItemResponse>>;
