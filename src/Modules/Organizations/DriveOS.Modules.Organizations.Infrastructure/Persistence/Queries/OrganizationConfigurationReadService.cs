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
    public async Task<OrganizationConfigurationResponse?> GetByIdAsync(
        OrganizationId organizationId,
        OrganizationConfigurationId configurationId,
        CancellationToken cancellationToken = default)
    {
        // Status is persisted as a string ("Draft", "Published", ...).
        // Casting the enum to int inside the EF projection makes Npgsql try
        // to cast that text column to integer (e.g. "Draft"::integer), which
        // fails with PostgreSQL 22P02. Materialize first, then cast in memory.
        var configuration = await dbContext.OrganizationConfigurations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate =>
                    candidate.OrganizationId == organizationId &&
                    candidate.Id == configurationId,
                cancellationToken);

        if (configuration is null)
        {
            return null;
        }

        return new OrganizationConfigurationResponse(
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
                : null);
    }

    public async Task<IReadOnlyList<OrganizationConfigurationListItemResponse>> GetVersionsAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default)
    {
        var configurations = await dbContext.OrganizationConfigurations
            .AsNoTracking()
            .Where(configuration => configuration.OrganizationId == organizationId)
            .OrderByDescending(configuration => configuration.VersionNumber)
            .ToListAsync(cancellationToken);

        return configurations
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
            .ToList();
    }
}
