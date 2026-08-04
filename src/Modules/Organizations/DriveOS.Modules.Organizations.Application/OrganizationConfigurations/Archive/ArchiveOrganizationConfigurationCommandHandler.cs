using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Effective;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Archive;

public sealed class ArchiveOrganizationConfigurationCommandHandler(
    IOrganizationConfigurationRepository repository,
    IUnitOfWork unitOfWork,
    IOrganizationConfigurationCacheInvalidator cacheInvalidator)
    : ICommandHandler<ArchiveOrganizationConfigurationCommand>
{
    public async Task<Result> Handle(
        ArchiveOrganizationConfigurationCommand command,
        CancellationToken cancellationToken)
    {
        OrganizationConfiguration? configuration = await repository.GetForUpdateAsync(
            command.ConfigurationId,
            command.OrganizationId,
            cancellationToken);

        if (configuration is null)
            return Result.Failure(OrganizationConfigurationErrors.NotFound);

        if (configuration.Revision != command.ExpectedRevision)
            return Result.Failure(OrganizationConfigurationErrors.ConcurrentUpdate);

        Result result = configuration.Archive();
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);
        cacheInvalidator.Invalidate(command.OrganizationId);
        return Result.Success();
    }
}
