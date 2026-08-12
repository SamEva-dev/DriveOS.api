using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Domain.Networks;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.Networks.AddNetworkMember;

internal sealed class AddNetworkMemberCommandHandler(
    IOrganizationRepository organizations,
    INetworkOrganizationMembershipRepository memberships,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<AddNetworkMemberCommand, NetworkOrganizationMembershipId>
{
    public async Task<Result<NetworkOrganizationMembershipId>> Handle(
        AddNetworkMemberCommand command, CancellationToken cancellationToken)
    {
        Organization? network = await organizations.GetByIdAsync(
            command.NetworkOrganizationId, true, cancellationToken);
        if (network?.Type != OrganizationType.DrivingSchoolNetwork)
            return Result.Failure<NetworkOrganizationMembershipId>(
                NetworkOrganizationMembershipErrors.CurrentOrganizationMustBeNetwork);

        Organization? member = await organizations.GetByIdAsync(
            command.MemberOrganizationId, true, cancellationToken);
        if (member is null)
            return Result.Failure<NetworkOrganizationMembershipId>(
                NetworkOrganizationMembershipErrors.MemberOrganizationNotFound);
        if (member.Type != OrganizationType.DrivingSchool)
            return Result.Failure<NetworkOrganizationMembershipId>(
                NetworkOrganizationMembershipErrors.MemberOrganizationMustBeDrivingSchool);
        if (member.Status == OrganizationStatus.Closed)
            return Result.Failure<NetworkOrganizationMembershipId>(
                NetworkOrganizationMembershipErrors.MemberOrganizationNotFound);
        if (await memberships.HasActiveMembershipAsync(command.MemberOrganizationId, cancellationToken))
            return Result.Failure<NetworkOrganizationMembershipId>(
                NetworkOrganizationMembershipErrors.ActiveMembershipAlreadyExists);

        NetworkOrganizationMembershipId id = NetworkOrganizationMembershipId.New();
        Result<NetworkOrganizationMembership> created = NetworkOrganizationMembership.Create(
            id, command.NetworkOrganizationId, command.MemberOrganizationId, clock.UtcNow);
        if (created.IsFailure)
            return Result.Failure<NetworkOrganizationMembershipId>(created.Error);

        await memberships.AddAsync(created.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(id);
    }
}
