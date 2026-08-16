using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateRegionalSettings;

public sealed class UpdateOrganizationRegionalSettingsCommandHandler(
    IOrganizationSettingsRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateOrganizationRegionalSettingsCommand>
{
    public async Task<Result> Handle(
        UpdateOrganizationRegionalSettingsCommand command,
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

        Result<OrganizationRegionalSettings> valueResult = OrganizationRegionalSettings.Create(
            command.DefaultLanguage,
            command.SupportedLanguages,
            command.TimeZoneId,
            command.CurrencyCode,
            command.DateFormat,
            command.TimeFormat,
            command.FirstDayOfWeek,
            command.MeasurementSystem
        );
        if (valueResult.IsFailure)
            return Result.Failure(valueResult.Error);

        Result updateResult = settings.UpdateRegionalSettings(valueResult.Value);
        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
