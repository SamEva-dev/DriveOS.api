using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.CreateDraft;

public sealed class CreateOrganizationConfigurationDraftCommandHandler(
    IOrganizationReadService organizationReadService,
    IOrganizationConfigurationRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<CreateOrganizationConfigurationDraftCommand, OrganizationConfigurationId>
{
    public async Task<Result<OrganizationConfigurationId>> Handle(
        CreateOrganizationConfigurationDraftCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure<OrganizationConfigurationId>(
                OrganizationConfigurationErrors.CurrentUserRequired);
        }

        var organization = await organizationReadService.GetByIdAsync(
            command.OrganizationId,
            cancellationToken);

        if (organization is null)
        {
            return Result.Failure<OrganizationConfigurationId>(OrganizationErrors.NotFound);
        }

        if (await repository.VersionExistsAsync(
                command.OrganizationId,
                command.VersionNumber,
                cancellationToken))
        {
            return Result.Failure<OrganizationConfigurationId>(
                OrganizationConfigurationErrors.VersionAlreadyExists);
        }

        Result<ConfigurationPayload> payloadResult =
            ConfigurationPayload.Create(command.PayloadJson);

        if (payloadResult.IsFailure)
        {
            return Result.Failure<OrganizationConfigurationId>(payloadResult.Error);
        }

        Result<OrganizationConfiguration> creationResult =
            OrganizationConfiguration.CreateDraft(
                OrganizationConfigurationId.New(),
                command.OrganizationId,
                command.VersionNumber,
                command.CountryCode,
                payloadResult.Value);

        if (creationResult.IsFailure)
        {
            return Result.Failure<OrganizationConfigurationId>(creationResult.Error);
        }

        await repository.AddAsync(creationResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(creationResult.Value.Id);
    }
}
