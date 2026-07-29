using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.Branches.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application
    .Branches.GetBranchById;

public sealed record GetBranchByIdQuery(
    OrganizationId OrganizationId,
    BranchId BranchId)
    : IQuery<BranchResponse>;
