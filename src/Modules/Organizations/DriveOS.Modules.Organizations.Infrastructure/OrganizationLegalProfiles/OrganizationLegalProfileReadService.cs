using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Models;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationLegalProfiles;

internal sealed class OrganizationLegalProfileReadService(OrganizationsDbContext dbContext)
    : IOrganizationLegalProfileReadService
{
    public Task<OrganizationLegalProfileResponse?> GetByOrganizationIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    ) =>
        dbContext
            .OrganizationLegalProfiles.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .Select(x => new OrganizationLegalProfileResponse(
                x.Id,
                x.OrganizationId,
                x.LegalForm,
                x.RegistrationNumber,
                x.TaxNumber,
                x.TradeName,
                x.IncorporationDate,
                x.RegisteredAddress.Line1,
                x.RegisteredAddress.Line2,
                x.RegisteredAddress.PostalCode,
                x.RegisteredAddress.City,
                x.RegisteredAddress.Region,
                x.RegisteredAddress.CountryCode,
                x.Status,
                x.Revision,
                x.CreatedAtUtc,
                x.CreatedByUserId,
                x.LastModifiedAtUtc,
                x.LastModifiedByUserId
            ))
            .SingleOrDefaultAsync(cancellationToken);
}
