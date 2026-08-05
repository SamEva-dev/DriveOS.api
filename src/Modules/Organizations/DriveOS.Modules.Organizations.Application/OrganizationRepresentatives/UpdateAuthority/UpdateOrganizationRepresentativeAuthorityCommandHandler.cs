using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.UpdateAuthority;

internal sealed class UpdateOrganizationRepresentativeAuthorityCommandHandler(
    IOrganizationRepresentativeRepository repository,
    OrganizationRepresentativeAccessSynchronizationService accessSynchronizationService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateOrganizationRepresentativeAuthorityCommand>
{
    public async Task<Result> Handle(UpdateOrganizationRepresentativeAuthorityCommand command, CancellationToken cancellationToken)
    {
        OrganizationRepresentative? representative = await repository.GetForUpdateAsync(
            command.RepresentativeId,
            command.OrganizationId,
            cancellationToken);

        if (representative is null)
            return Result.Failure(OrganizationRepresentativeErrors.NotFound);
        if (representative.Revision != command.ExpectedRevision)
            return Result.Failure(OrganizationRepresentativeErrors.ConcurrentUpdate);

        Result<RepresentativeAuthorityScope> scope = RepresentativeAuthorityScope.Create(command.AuthorityScope);
        if (scope.IsFailure)
            return Result.Failure(scope.Error);

        Result result = representative.UpdateAuthority(
            scope.Value,
            command.UserId,
            command.EffectiveFrom,
            command.EffectiveTo);

        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);
        await accessSynchronizationService.SynchronizeAsync(representative, cancellationToken);
        return Result.Success();
    }
}
