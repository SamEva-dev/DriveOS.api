using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application
    .Organizations.GetOrganizationById;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure
    .Persistence.Queries;

internal sealed class OrganizationReadService
    : IOrganizationReadService
{
    private readonly OrganizationsDbContext _dbContext;

    public OrganizationReadService(
        OrganizationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<OrganizationResponse?> GetByIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Organizations
            .AsNoTracking()
            .Where(organization =>
                organization.Id == organizationId)
            .Select(organization =>
                new OrganizationResponse(
                    organization.Id.Value,
                    organization.LegalName,
                    organization.CountryCode,
                    organization.Type.ToString(),
                    organization.Status.ToString(),
                    organization.CreatedAtUtc,
                    organization.CreatedByUserId.HasValue
                        ? organization
                            .CreatedByUserId.Value.Value
                        : null,
                    organization.LastModifiedAtUtc,
                    organization.LastModifiedByUserId.HasValue
                        ? organization
                            .LastModifiedByUserId.Value.Value
                        : null))
            .SingleOrDefaultAsync(cancellationToken);
    }
}