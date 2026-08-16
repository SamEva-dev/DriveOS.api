using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Repositories;

internal sealed class OrganizationLegalProfileRepository(OrganizationsDbContext dbContext)
    : IOrganizationLegalProfileRepository
{
    public Task<OrganizationLegalProfile?> GetForUpdateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    ) =>
        dbContext.OrganizationLegalProfiles.SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId,
            cancellationToken
        );

    public Task<bool> RegistrationNumberExistsAsync(
        string countryCode,
        string registrationNumber,
        OrganizationId? excludingOrganizationId = null,
        CancellationToken cancellationToken = default
    )
    {
        string normalizedCountryCode = countryCode.Trim().ToUpperInvariant();
        string normalizedRegistrationNumber = registrationNumber.Trim().ToUpperInvariant();

        return dbContext
            .OrganizationLegalProfiles.AsNoTracking()
            .AnyAsync(
                x =>
                    x.RegisteredAddress.CountryCode == normalizedCountryCode
                    && x.RegistrationNumber == normalizedRegistrationNumber
                    && (
                        !excludingOrganizationId.HasValue
                        || x.OrganizationId != excludingOrganizationId.Value
                    ),
                cancellationToken
            );
    }

    public Task AddAsync(
        OrganizationLegalProfile legalProfile,
        CancellationToken cancellationToken = default
    ) => dbContext.OrganizationLegalProfiles.AddAsync(legalProfile, cancellationToken).AsTask();
}
