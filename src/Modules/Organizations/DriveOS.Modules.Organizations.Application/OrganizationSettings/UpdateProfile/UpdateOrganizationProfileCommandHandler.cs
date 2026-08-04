using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateProfile;

public sealed class UpdateOrganizationProfileCommandHandler(
    IOrganizationSettingsRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateOrganizationProfileCommand>
{
    public async Task<Result> Handle(
        UpdateOrganizationProfileCommand command,
        CancellationToken cancellationToken)
    {
        var settings = await repository.GetForUpdateAsync(
            command.OrganizationId,
            cancellationToken);

        if (settings is null)
        {
            return Result.Failure(OrganizationSettingsErrors.NotFound);
        }

        if (settings.Version != command.ExpectedVersion)
        {
            return Result.Failure(OrganizationSettingsErrors.ConcurrentUpdate);
        }

        Result<OrganizationProfile> profileResult = OrganizationProfile.Create(
            command.TradeName,
            command.RegistrationNumber,
            command.TaxNumber);

        if (profileResult.IsFailure)
        {
            return Result.Failure(profileResult.Error);
        }

        Result updateResult = settings.UpdateProfile(profileResult.Value);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
