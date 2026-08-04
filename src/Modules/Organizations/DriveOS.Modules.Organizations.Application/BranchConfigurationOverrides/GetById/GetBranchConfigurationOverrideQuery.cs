using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Models;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.GetById;
public sealed record GetBranchConfigurationOverrideQuery(
    OrganizationId OrganizationId, BranchId BranchId, BranchConfigurationOverrideId OverrideId)
    : IQuery<BranchConfigurationOverrideResponse>;
