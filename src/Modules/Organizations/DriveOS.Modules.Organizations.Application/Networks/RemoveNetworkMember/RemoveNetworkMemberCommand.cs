using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Networks.RemoveNetworkMember;

public sealed record RemoveNetworkMemberCommand(
    OrganizationId NetworkOrganizationId,
    OrganizationId MemberOrganizationId
) : ICommand;
