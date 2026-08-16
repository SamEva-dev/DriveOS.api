using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateContact;

public sealed class UpdateOrganizationContactCommandHandler(
    IOrganizationSettingsRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateOrganizationContactCommand>
{
    public async Task<Result> Handle(
        UpdateOrganizationContactCommand command,
        CancellationToken cancellationToken
    )
    {
        var settings = await repository.GetForUpdateAsync(
            command.OrganizationId,
            cancellationToken
        );
        if (settings is null)
            return Result.Failure(OrganizationSettingsErrors.NotFound);
        if (settings.Version != command.ExpectedVersion)
            return Result.Failure(OrganizationSettingsErrors.ConcurrentUpdate);

        Result<OrganizationContactInformation> valueResult = OrganizationContactInformation.Create(
            command.Email,
            command.Phone,
            command.Website
        );
        if (valueResult.IsFailure)
            return Result.Failure(valueResult.Error);

        Result updateResult = settings.UpdateContact(valueResult.Value);
        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
