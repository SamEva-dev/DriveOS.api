using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Update;

internal sealed class UpdateOrganizationLegalProfileCommandHandler(
    IOrganizationReadService organizationReadService,
    IOrganizationLegalProfileRepository repository,
    IOrganizationActivationReadinessCacheInvalidator readinessCacheInvalidator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<UpdateOrganizationLegalProfileCommand>
{
    public async Task<Result> Handle(
        UpdateOrganizationLegalProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure(OrganizationLegalProfileErrors.CurrentUserRequired);
        var organization = await organizationReadService.GetByIdAsync(
            command.OrganizationId,
            cancellationToken
        );
        if (organization is null)
            return Result.Failure(OrganizationErrors.NotFound);
        if (organization.Status is "Closed" or "Archived")
            return Result.Failure(OrganizationLegalProfileErrors.OrganizationUnavailable);
        var profile = await repository.GetForUpdateAsync(command.OrganizationId, cancellationToken);
        if (profile is null)
            return Result.Failure(OrganizationLegalProfileErrors.NotFound);
        if (profile.Revision != command.ExpectedRevision)
            return Result.Failure(OrganizationLegalProfileErrors.ConcurrentUpdate);
        string countryCode = organization.CountryCode.Trim().ToUpperInvariant();
        string registrationNumber = command.RegistrationNumber.Trim().ToUpperInvariant();
        if (
            await repository.RegistrationNumberExistsAsync(
                countryCode,
                registrationNumber,
                command.OrganizationId,
                cancellationToken
            )
        )
            return Result.Failure(OrganizationLegalProfileErrors.DuplicateRegistrationNumber);
        Result<RegisteredAddress> addressResult = RegisteredAddress.Create(
            command.AddressLine1,
            command.AddressLine2,
            command.PostalCode,
            command.City,
            command.Region,
            command.CountryCode
        );
        if (addressResult.IsFailure)
            return Result.Failure(addressResult.Error);
        Result result = profile.Update(
            command.LegalForm,
            command.RegistrationNumber,
            command.TaxNumber,
            command.TradeName,
            command.IncorporationDate,
            addressResult.Value,
            countryCode
        );
        if (result.IsFailure)
            return result;
        await unitOfWork.CommitAsync(cancellationToken);
        readinessCacheInvalidator.Invalidate(command.OrganizationId);
        return Result.Success();
    }
}
