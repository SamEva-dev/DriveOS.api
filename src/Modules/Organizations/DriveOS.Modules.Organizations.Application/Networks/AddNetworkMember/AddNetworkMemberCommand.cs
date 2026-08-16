using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Networks.AddNetworkMember;

public sealed record AddNetworkMemberCommand(
    OrganizationId NetworkOrganizationId,
    OrganizationId MemberOrganizationId
) : ICommand<NetworkOrganizationMembershipId>;
