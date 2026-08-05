using DriveOS.Modules.Organizations.Application.OrganizationClosures.Commands;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationClosures;

/// <summary>
/// Conservative anonymization adapter. It never deletes legal, billing or audit evidence.
/// The actual PII anonymizers must be supplied by the owning bounded contexts.
/// </summary>
internal sealed class OrganizationAnonymizationService(
    OrganizationsDbContext dbContext,
    ILogger<OrganizationAnonymizationService> logger)
    : IOrganizationAnonymizationService
{
    public Task<bool> HasIrreversibleAnonymizationStartedAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken) =>
        dbContext.OrganizationClosures
            .AsNoTracking()
            .AnyAsync(
                x => x.OrganizationId == organizationId &&
                     x.Status == OrganizationClosureStatus.Completed &&
                     x.DataDisposition == OrganizationDataDisposition.AnonymizeAfterRetention &&
                     x.RetentionUntilUtc != null &&
                     x.RetentionUntilUtc <= DateTimeOffset.UtcNow,
                cancellationToken);

    public Task AnonymizeAsync(
        OrganizationId organizationId,
        DateTimeOffset dueAtUtc,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (dueAtUtc > DateTimeOffset.UtcNow)
            throw new InvalidOperationException("The anonymization retention date has not been reached.");

        logger.LogWarning(
            "Organization {OrganizationId} reached its anonymization date {DueAtUtc}. " +
            "No destructive action was executed by Organization & Tenancy; owning bounded-context anonymizers must process their PII.",
            organizationId,
            dueAtUtc);

        return Task.CompletedTask;
    }
}
