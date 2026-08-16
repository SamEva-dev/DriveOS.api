using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Domain.Networks;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.Networks.RemoveNetworkMember;

internal sealed class RemoveNetworkMemberCommandHandler(
    IOrganizationRepository organizations,
    INetworkOrganizationMembershipRepository memberships,
    IUnitOfWork unitOfWork,
    IClock clock
) : ICommandHandler<RemoveNetworkMemberCommand>
{
    public async Task<Result> Handle(
        RemoveNetworkMemberCommand command,
        CancellationToken cancellationToken
    )
    {
        Organization? network = await organizations.GetByIdAsync(
            command.NetworkOrganizationId,
            true,
            cancellationToken
        );
        if (network?.Type != OrganizationType.DrivingSchoolNetwork)
            return Result.Failure(
                NetworkOrganizationMembershipErrors.CurrentOrganizationMustBeNetwork
            );

        NetworkOrganizationMembership? membership = await memberships.GetActiveAsync(
            command.NetworkOrganizationId,
            command.MemberOrganizationId,
            cancellationToken
        );
        if (membership is null)
            return Result.Failure(NetworkOrganizationMembershipErrors.ActiveMembershipNotFound);

        Result ended = membership.End(clock.UtcNow);
        if (ended.IsFailure)
            return ended;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
