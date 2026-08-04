using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Effective;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Archive;

public sealed class ArchiveBranchConfigurationOverrideCommandHandler(
    IBranchConfigurationOverrideRepository repository,
    IOrganizationConfigurationCacheInvalidator cacheInvalidator,
    IUnitOfWork unitOfWork) : ICommandHandler<ArchiveBranchConfigurationOverrideCommand>
{
    public async Task<Result> Handle(
        ArchiveBranchConfigurationOverrideCommand command,
        CancellationToken cancellationToken)
    {
        BranchConfigurationOverride? branchOverride = await repository.GetForUpdateAsync(
            command.OverrideId,
            command.OrganizationId,
            command.BranchId,
            cancellationToken);

        if (branchOverride is null)
            return Result.Failure(BranchConfigurationOverrideErrors.NotFound);

        if (branchOverride.Revision != command.ExpectedRevision)
            return Result.Failure(BranchConfigurationOverrideErrors.ConcurrentUpdate);

        Result result = branchOverride.Archive();
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);
        cacheInvalidator.Invalidate(command.OrganizationId);
        return Result.Success();
    }
}
