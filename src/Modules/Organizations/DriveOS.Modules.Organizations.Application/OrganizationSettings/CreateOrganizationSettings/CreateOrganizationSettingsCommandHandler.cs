using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.CreateOrganizationSettings;

public sealed class CreateOrganizationSettingsCommandHandler(
    IOrganizationReadService organizationReadService,
    IBranchReadService branchReadService,
    IOrganizationSettingsRepository settingsRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrganizationSettingsCommand, OrganizationSettingsId>
{
    public async Task<Result<OrganizationSettingsId>> Handle(
        CreateOrganizationSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var organization = await organizationReadService.GetByIdAsync(
            command.OrganizationId,
            cancellationToken);

        if (organization is null)
        {
            return Result.Failure<OrganizationSettingsId>(
                OrganizationErrors.NotFound);
        }

        if (await settingsRepository.ExistsAsync(
                command.OrganizationId,
                cancellationToken))
        {
            return Result.Failure<OrganizationSettingsId>(
                OrganizationSettingsErrors.AlreadyExists);
        }

        if (command.DefaultBranchId is not null)
        {
            var branch = await branchReadService.GetByIdAsync(
                command.OrganizationId,
                command.DefaultBranchId.Value,
                cancellationToken);

            if (branch is null)
            {
                return Result.Failure<OrganizationSettingsId>(
                    OrganizationSettingsErrors.DefaultBranchNotOwned);
            }
        }

        Result<OrganizationProfile> profileResult = OrganizationProfile.Create(
            command.TradeName,
            command.RegistrationNumber,
            command.TaxNumber);

        if (profileResult.IsFailure)
        {
            return Result.Failure<OrganizationSettingsId>(profileResult.Error);
        }

        Result<OrganizationContactInformation> contactResult =
            OrganizationContactInformation.Create(
                command.Email,
                command.Phone,
                command.Website);

        if (contactResult.IsFailure)
        {
            return Result.Failure<OrganizationSettingsId>(contactResult.Error);
        }

        Result<OrganizationAddress> addressResult = OrganizationAddress.Create(
            command.AddressLine1,
            command.AddressLine2,
            command.PostalCode,
            command.City,
            command.Region,
            command.AddressCountryCode);

        if (addressResult.IsFailure)
        {
            return Result.Failure<OrganizationSettingsId>(addressResult.Error);
        }

        Result<OrganizationRegionalSettings> regionalResult =
            OrganizationRegionalSettings.Create(
                command.DefaultLanguage,
                command.SupportedLanguages,
                command.TimeZoneId,
                command.CurrencyCode,
                command.DateFormat,
                command.TimeFormat,
                command.FirstDayOfWeek,
                command.MeasurementSystem);

        if (regionalResult.IsFailure)
        {
            return Result.Failure<OrganizationSettingsId>(regionalResult.Error);
        }

        Result<OrganizationOperationalSettings> operationalResult =
            OrganizationOperationalSettings.Create(
                command.DefaultSessionDurationMinutes,
                command.DefaultBookingLeadTimeMinutes,
                command.DefaultCancellationDelayHours,
                command.AllowStudentSelfBooking,
                command.RequireBranchForOperations,
                command.DefaultBranchId);

        if (operationalResult.IsFailure)
        {
            return Result.Failure<OrganizationSettingsId>(operationalResult.Error);
        }

        Result<global::DriveOS.Modules.Organizations.Domain.OrganizationSettings.OrganizationSettings> creationResult =
            global::DriveOS.Modules.Organizations.Domain.OrganizationSettings.OrganizationSettings.Create(
                OrganizationSettingsId.New(),
                command.OrganizationId,
                profileResult.Value,
                contactResult.Value,
                addressResult.Value,
                regionalResult.Value,
                operationalResult.Value);

        if (creationResult.IsFailure)
        {
            return Result.Failure<OrganizationSettingsId>(creationResult.Error);
        }

        await settingsRepository.AddAsync(
            creationResult.Value,
            cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(creationResult.Value.Id);
    }
}
