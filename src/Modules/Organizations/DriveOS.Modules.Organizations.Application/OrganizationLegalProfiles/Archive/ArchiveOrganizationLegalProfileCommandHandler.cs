using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Archive;

internal sealed class ArchiveOrganizationLegalProfileCommandHandler(
    IOrganizationLegalProfileRepository repository,
    IOrganizationActivationReadinessCacheInvalidator readinessCacheInvalidator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<ArchiveOrganizationLegalProfileCommand>
{
    public async Task<Result> Handle(
        ArchiveOrganizationLegalProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure(OrganizationLegalProfileErrors.CurrentUserRequired);
        var profile = await repository.GetForUpdateAsync(command.OrganizationId, cancellationToken);
        if (profile is null)
            return Result.Failure(OrganizationLegalProfileErrors.NotFound);
        if (profile.Revision != command.ExpectedRevision)
            return Result.Failure(OrganizationLegalProfileErrors.ConcurrentUpdate);
        Result result = profile.Archive();
        if (result.IsFailure)
            return result;
        await unitOfWork.CommitAsync(cancellationToken);
        readinessCacheInvalidator.Invalidate(command.OrganizationId);
        return Result.Success();
    }
}
