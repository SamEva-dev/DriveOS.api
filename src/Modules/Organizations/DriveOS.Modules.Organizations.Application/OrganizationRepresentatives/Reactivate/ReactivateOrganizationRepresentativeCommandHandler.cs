using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Reactivate;

internal sealed class ReactivateOrganizationRepresentativeCommandHandler(
    IOrganizationRepresentativeRepository repository,
    OrganizationRepresentativeAccessSynchronizationService accessSynchronizationService,
    IOrganizationActivationReadinessCacheInvalidator readinessCacheInvalidator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<ReactivateOrganizationRepresentativeCommand>
{
    public async Task<Result> Handle(
        ReactivateOrganizationRepresentativeCommand c,
        CancellationToken ct
    )
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure(OrganizationRepresentativeErrors.CurrentUserRequired);
        var e = await repository.GetForUpdateAsync(c.RepresentativeId, c.OrganizationId, ct);
        if (e is null)
            return Result.Failure(OrganizationRepresentativeErrors.NotFound);
        if (e.Revision != c.ExpectedRevision)
            return Result.Failure(OrganizationRepresentativeErrors.ConcurrentUpdate);
        var r = e.Reactivate(c.Reason, currentUser.UserId.Value);
        if (r.IsFailure)
            return r;
        await unitOfWork.CommitAsync(ct);
        readinessCacheInvalidator.Invalidate(c.OrganizationId);
        await accessSynchronizationService.SynchronizeAsync(e, ct);
        return Result.Success();
    }
}
