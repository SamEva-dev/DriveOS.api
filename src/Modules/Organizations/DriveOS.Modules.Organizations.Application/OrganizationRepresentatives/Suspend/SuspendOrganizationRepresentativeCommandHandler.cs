using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Suspend;

internal sealed class SuspendOrganizationRepresentativeCommandHandler(
    IOrganizationRepresentativeRepository repository,
    OrganizationRepresentativeAccessSynchronizationService accessSynchronizationService,
    IOrganizationActivationReadinessCacheInvalidator readinessCacheInvalidator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<SuspendOrganizationRepresentativeCommand>
{
    public async Task<Result> Handle(
        SuspendOrganizationRepresentativeCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure(OrganizationRepresentativeErrors.CurrentUserRequired);
        }

        OrganizationRepresentative? representative = await repository.GetForUpdateAsync(
            command.RepresentativeId,
            command.OrganizationId,
            cancellationToken
        );

        if (representative is null)
        {
            return Result.Failure(OrganizationRepresentativeErrors.NotFound);
        }

        if (representative.Revision != command.ExpectedRevision)
        {
            return Result.Failure(OrganizationRepresentativeErrors.ConcurrentUpdate);
        }

        // Suspending the last active owner would violate the Organization invariant just as ending it would.
        if (representative.IsOwner)
        {
            int remainingActiveOwners = await repository.CountActiveOwnersAsync(
                command.OrganizationId,
                representative.Id,
                cancellationToken
            );

            if (remainingActiveOwners == 0)
            {
                return Result.Failure(
                    OrganizationRepresentativeErrors.LastActiveOwnerCannotBeEnded
                );
            }
        }

        Result result = representative.Suspend(command.Reason, currentUser.UserId.Value);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        readinessCacheInvalidator.Invalidate(command.OrganizationId);
        await accessSynchronizationService.SynchronizeAsync(representative, cancellationToken);

        return Result.Success();
    }
}
