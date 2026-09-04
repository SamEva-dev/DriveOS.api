using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.Organizations.ProvisionOrganization;

public sealed class ProvisionOrganizationCommandHandler(
    IOrganizationRepository organizationRepository,
    IOrganizationRepresentativeRepository representativeRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : ICommandHandler<ProvisionOrganizationCommand, ProvisionOrganizationResult>
{
    public async Task<Result<ProvisionOrganizationResult>> Handle(
        ProvisionOrganizationCommand command,
        CancellationToken cancellationToken
    )
    {
        string idempotencyKey = command.IdempotencyKey?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 200)
            return Result.Failure<ProvisionOrganizationResult>(
                OrganizationErrors.InvalidProvisioningKey
            );

        Organization? existing = await organizationRepository.GetByProvisioningKeyAsync(
            idempotencyKey,
            asNoTracking: true,
            cancellationToken
        );
        if (existing is not null)
        {
            bool sameRequest = existing.ProvisioningExternalUserId == command.ExternalUserId
                && string.Equals(
                    existing.LegalName,
                    command.LegalName.Trim(),
                    StringComparison.Ordinal
                )
                && string.Equals(
                    existing.CountryCode,
                    command.CountryCode.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )
                && (int)existing.Type == command.OrganizationType;
            return sameRequest
                ? Result.Success(
                    new ProvisionOrganizationResult(
                        existing.Id,
                        existing.Status.ToString(),
                        false
                    )
                )
                : Result.Failure<ProvisionOrganizationResult>(
                    OrganizationErrors.ProvisioningKeyConflict
                );
        }

        if (command.ExternalUserId.IsEmpty)
            return Result.Failure<ProvisionOrganizationResult>(
                OrganizationErrors.InvalidProvisioningExternalUserId
            );

        string legalName = command.LegalName.Trim();
        string countryCode = command.CountryCode.Trim().ToUpperInvariant();
        if (
            await organizationRepository.ExistsByLegalNameAsync(
                legalName,
                countryCode,
                cancellationToken
            )
        )
            return Result.Failure<ProvisionOrganizationResult>(
                OrganizationErrors.LegalNameAlreadyExists
            );

        Result<Organization> organizationResult = Organization.Create(
            OrganizationId.New(),
            legalName,
            countryCode,
            (OrganizationType)command.OrganizationType
        );
        if (organizationResult.IsFailure)
            return Result.Failure<ProvisionOrganizationResult>(organizationResult.Error);

        Result provisioningIdentityResult = organizationResult.Value.SetProvisioningIdentity(
            command.ExternalUserId,
            idempotencyKey
        );
        if (provisioningIdentityResult.IsFailure)
            return Result.Failure<ProvisionOrganizationResult>(provisioningIdentityResult.Error);

        Result<RepresentativeAuthorityScope> scopeResult = RepresentativeAuthorityScope.Create(
            "Full legal and administrative authority"
        );
        if (scopeResult.IsFailure)
            return Result.Failure<ProvisionOrganizationResult>(scopeResult.Error);

        Result<OrganizationRepresentative> representativeResult =
            OrganizationRepresentative.Create(
                OrganizationRepresentativeId.New(),
                organizationResult.Value.Id,
                new PersonId(command.ExternalUserId.Value),
                command.ExternalUserId,
                OrganizationRepresentativeType.Owner,
                scopeResult.Value,
                isPrimaryOwner: true,
                DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime),
                effectiveTo: null
            );
        if (representativeResult.IsFailure)
            return Result.Failure<ProvisionOrganizationResult>(representativeResult.Error);

        Result activationResult = representativeResult.Value.Activate();
        if (activationResult.IsFailure)
            return Result.Failure<ProvisionOrganizationResult>(activationResult.Error);

        DateTimeOffset now = timeProvider.GetUtcNow();
        organizationResult.Value.SetCreatedAudit(now, command.ExternalUserId);
        representativeResult.Value.SetCreatedAudit(now, command.ExternalUserId);

        await organizationRepository.AddAsync(organizationResult.Value, cancellationToken);
        await representativeRepository.AddAsync(representativeResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(
            new ProvisionOrganizationResult(
                organizationResult.Value.Id,
                organizationResult.Value.Status.ToString(),
                true
            )
        );
    }
}
