using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.SetPrimaryOwner;

internal sealed class SetPrimaryOrganizationOwnerCommandHandler(
    IOrganizationRepresentativeRepository repository,
    OrganizationRepresentativeAccessSynchronizationService accessSynchronizationService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SetPrimaryOrganizationOwnerCommand>
{
    public async Task<Result> Handle(SetPrimaryOrganizationOwnerCommand command, CancellationToken cancellationToken)
    {
        OrganizationRepresentative? target = await repository.GetForUpdateAsync(
            command.RepresentativeId,
            command.OrganizationId,
            cancellationToken);

        if (target is null)
            return Result.Failure(OrganizationRepresentativeErrors.NotFound);
        if (target.Revision != command.ExpectedRevision)
            return Result.Failure(OrganizationRepresentativeErrors.ConcurrentUpdate);

        OrganizationRepresentative? current = await repository.GetPrimaryOwnerForUpdateAsync(
            command.OrganizationId,
            cancellationToken);

        if (current is not null && current.Id != target.Id)
            current.ClearPrimaryOwner();

        Result result = target.SetPrimaryOwner();
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);

        if (current is not null && current.Id != target.Id)
            await accessSynchronizationService.SynchronizeAsync(current, cancellationToken);

        await accessSynchronizationService.SynchronizeAsync(target, cancellationToken);
        return Result.Success();
    }
}
