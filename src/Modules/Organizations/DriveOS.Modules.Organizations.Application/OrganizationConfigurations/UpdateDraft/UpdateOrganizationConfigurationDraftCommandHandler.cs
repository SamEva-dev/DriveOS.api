using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.UpdateDraft;

public sealed class UpdateOrganizationConfigurationDraftCommandHandler(
    IOrganizationConfigurationRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateOrganizationConfigurationDraftCommand>
{
    public async Task<Result> Handle(
        UpdateOrganizationConfigurationDraftCommand command,
        CancellationToken cancellationToken
    )
    {
        OrganizationConfiguration? configuration = await repository.GetForUpdateAsync(
            command.ConfigurationId,
            command.OrganizationId,
            cancellationToken
        );

        if (configuration is null)
            return Result.Failure(OrganizationConfigurationErrors.NotFound);

        if (configuration.Revision != command.ExpectedRevision)
            return Result.Failure(OrganizationConfigurationErrors.ConcurrentUpdate);

        Result<ConfigurationPayload> payloadResult = ConfigurationPayload.Create(
            command.PayloadJson
        );

        if (payloadResult.IsFailure)
            return Result.Failure(payloadResult.Error);

        Result result = configuration.UpdateDraft(payloadResult.Value);
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
