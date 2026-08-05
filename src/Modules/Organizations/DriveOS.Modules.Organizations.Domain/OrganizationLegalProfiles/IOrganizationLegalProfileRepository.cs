using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;

public interface IOrganizationLegalProfileRepository
{
    Task<OrganizationLegalProfile?> GetForUpdateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> RegistrationNumberExistsAsync(
        string countryCode,
        string registrationNumber,
        OrganizationId? excludingOrganizationId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        OrganizationLegalProfile legalProfile,
        CancellationToken cancellationToken = default);
}
