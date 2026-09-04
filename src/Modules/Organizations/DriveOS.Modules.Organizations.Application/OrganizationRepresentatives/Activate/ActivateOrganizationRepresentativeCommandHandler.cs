using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Activate;

internal sealed class ActivateOrganizationRepresentativeCommandHandler(
    IOrganizationRepresentativeRepository repository,
    OrganizationRepresentativeAccessSynchronizationService accessSynchronizationService,
    IOrganizationActivationReadinessCacheInvalidator readinessCacheInvalidator,
    IUnitOfWork unitOfWork
) : ICommandHandler<ActivateOrganizationRepresentativeCommand>
{
    public async Task<Result> Handle(
        ActivateOrganizationRepresentativeCommand command,
        CancellationToken cancellationToken
    )
    {
        OrganizationRepresentative? representative = await repository.GetForUpdateAsync(
            command.RepresentativeId,
            command.OrganizationId,
            cancellationToken
        );

        if (representative is null)
            return Result.Failure(OrganizationRepresentativeErrors.NotFound);
        if (representative.Revision != command.ExpectedRevision)
            return Result.Failure(OrganizationRepresentativeErrors.ConcurrentUpdate);

        Result result = representative.Activate();
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);
        readinessCacheInvalidator.Invalidate(command.OrganizationId);
        await accessSynchronizationService.SynchronizeAsync(representative, cancellationToken);
        return Result.Success();
    }
}
