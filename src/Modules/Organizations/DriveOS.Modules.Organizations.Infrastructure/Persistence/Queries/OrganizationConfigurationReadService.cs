using DriveOS.Modules.Organizations.Application.OrganizationConfigurations;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Queries;

internal sealed class OrganizationConfigurationReadService(
    OrganizationsDbContext dbContext)
    : IOrganizationConfigurationReadService
{
    public Task<OrganizationConfigurationResponse?> GetByIdAsync(
        OrganizationId organizationId,
        OrganizationConfigurationId configurationId,
        CancellationToken cancellationToken = default) =>
        dbContext.OrganizationConfigurations
            .AsNoTracking()
            .Where(configuration =>
                configuration.OrganizationId == organizationId &&
                configuration.Id == configurationId)
            .Select(configuration => new OrganizationConfigurationResponse(
                configuration.Id.Value,
                configuration.OrganizationId.Value,
                configuration.VersionNumber,
                configuration.CountryCode,
                configuration.Payload.Json,
                (int)configuration.Status,
                configuration.EffectiveFromUtc,
                configuration.EffectiveToUtc,
                configuration.PublishedAtUtc,
                configuration.PublishedByUserId.HasValue
                    ? configuration.PublishedByUserId.Value.Value
                    : null,
                configuration.Revision,
                configuration.CreatedAtUtc,
                configuration.CreatedByUserId.HasValue
                    ? configuration.CreatedByUserId.Value.Value
                    : null,
                configuration.LastModifiedAtUtc,
                configuration.LastModifiedByUserId.HasValue
                    ? configuration.LastModifiedByUserId.Value.Value
                    : null))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<OrganizationConfigurationListItemResponse>> GetVersionsAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default) =>
        await dbContext.OrganizationConfigurations
            .AsNoTracking()
            .Where(configuration => configuration.OrganizationId == organizationId)
            .OrderByDescending(configuration => configuration.VersionNumber)
            .Select(configuration => new OrganizationConfigurationListItemResponse(
                configuration.Id.Value,
                configuration.VersionNumber,
                configuration.CountryCode,
                (int)configuration.Status,
                configuration.EffectiveFromUtc,
                configuration.EffectiveToUtc,
                configuration.PublishedAtUtc,
                configuration.Revision,
                configuration.CreatedAtUtc))
            .ToListAsync(cancellationToken);
}
