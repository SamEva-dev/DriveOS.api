using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;
using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Create;

internal sealed class CreateOrganizationLegalProfileCommandHandler(
    IOrganizationReadService organizationReadService,
    IOrganizationLegalProfileRepository repository,
    IOrganizationLegalProfileComplianceService complianceService,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<CreateOrganizationLegalProfileCommand, OrganizationLegalProfileId>
{
    public async Task<Result<OrganizationLegalProfileId>> Handle(
        CreateOrganizationLegalProfileCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure<OrganizationLegalProfileId>(OrganizationLegalProfileErrors.CurrentUserRequired);

        var organization = await organizationReadService.GetByIdAsync(command.OrganizationId, cancellationToken);
        if (organization is null)
            return Result.Failure<OrganizationLegalProfileId>(OrganizationErrors.NotFound);
        if (organization.Status is "Closed" or "Archived")
            return Result.Failure<OrganizationLegalProfileId>(OrganizationLegalProfileErrors.OrganizationUnavailable);

        if (await repository.GetForUpdateAsync(command.OrganizationId, cancellationToken) is not null)
            return Result.Failure<OrganizationLegalProfileId>(OrganizationLegalProfileErrors.AlreadyExists);

        string countryCode = organization.CountryCode.Trim().ToUpperInvariant();
        string registrationNumber = command.RegistrationNumber.Trim().ToUpperInvariant();
        if (await repository.RegistrationNumberExistsAsync(countryCode, registrationNumber, null, cancellationToken))
            return Result.Failure<OrganizationLegalProfileId>(OrganizationLegalProfileErrors.DuplicateRegistrationNumber);

        Result<RegisteredAddress> addressResult = RegisteredAddress.Create(
            command.AddressLine1, command.AddressLine2, command.PostalCode, command.City, command.Region, command.CountryCode);
        if (addressResult.IsFailure)
            return Result.Failure<OrganizationLegalProfileId>(addressResult.Error);

        Result<OrganizationLegalProfile> result = OrganizationLegalProfile.Create(
            OrganizationLegalProfileId.New(), command.OrganizationId, command.LegalForm, command.RegistrationNumber,
            command.TaxNumber, command.TradeName, command.IncorporationDate, addressResult.Value, countryCode);
        if (result.IsFailure)
            return Result.Failure<OrganizationLegalProfileId>(result.Error);

        if (command.ActivateImmediately)
        {
            OrganizationLegalProfileComplianceResult compliance = complianceService.Validate(result.Value);
            if (!compliance.IsCompliant)
                return Result.Failure<OrganizationLegalProfileId>(
                    OrganizationLegalProfileComplianceErrors.ActivationBlocked(compliance.Issues));

            Result activation = result.Value.Activate();
            if (activation.IsFailure)
                return Result.Failure<OrganizationLegalProfileId>(activation.Error);
        }

        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}
