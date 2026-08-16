using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Create;

internal sealed class CreateOrganizationRepresentativeCommandHandler(
    IOrganizationReadService organizationReadService,
    IOrganizationRepresentativeRepository repository,
    OrganizationRepresentativeAccessSynchronizationService accessSynchronizationService,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<CreateOrganizationRepresentativeCommand, OrganizationRepresentativeId>
{
    public async Task<Result<OrganizationRepresentativeId>> Handle(
        CreateOrganizationRepresentativeCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure<OrganizationRepresentativeId>(
                OrganizationRepresentativeErrors.CurrentUserRequired
            );

        var organization = await organizationReadService.GetByIdAsync(
            command.OrganizationId,
            cancellationToken
        );
        if (organization is null)
            return Result.Failure<OrganizationRepresentativeId>(OrganizationErrors.NotFound);
        if (organization.Status is "Closed" or "Archived")
            return Result.Failure<OrganizationRepresentativeId>(
                OrganizationRepresentativeErrors.OrganizationUnavailable
            );

        if (
            await repository.ExistsActiveAsync(
                command.OrganizationId,
                command.PersonId,
                command.RepresentativeType,
                cancellationToken
            )
        )
            return Result.Failure<OrganizationRepresentativeId>(
                OrganizationRepresentativeErrors.DuplicateActiveRepresentation
            );

        Result<RepresentativeAuthorityScope> scopeResult = RepresentativeAuthorityScope.Create(
            command.AuthorityScope
        );
        if (scopeResult.IsFailure)
            return Result.Failure<OrganizationRepresentativeId>(scopeResult.Error);

        if (command.IsPrimaryOwner)
        {
            OrganizationRepresentative? currentPrimary =
                await repository.GetPrimaryOwnerForUpdateAsync(
                    command.OrganizationId,
                    cancellationToken
                );
            if (currentPrimary is not null)
                currentPrimary.ClearPrimaryOwner();
        }

        Result<OrganizationRepresentative> result = OrganizationRepresentative.Create(
            OrganizationRepresentativeId.New(),
            command.OrganizationId,
            command.PersonId,
            command.UserId,
            command.RepresentativeType,
            scopeResult.Value,
            command.IsPrimaryOwner,
            command.EffectiveFrom,
            command.EffectiveTo
        );
        if (result.IsFailure)
            return Result.Failure<OrganizationRepresentativeId>(result.Error);

        if (command.ActivateImmediately)
        {
            Result activateResult = result.Value.Activate();
            if (activateResult.IsFailure)
                return Result.Failure<OrganizationRepresentativeId>(activateResult.Error);
        }

        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        await accessSynchronizationService.SynchronizeAsync(result.Value, cancellationToken);
        return Result.Success(result.Value.Id);
    }
}
