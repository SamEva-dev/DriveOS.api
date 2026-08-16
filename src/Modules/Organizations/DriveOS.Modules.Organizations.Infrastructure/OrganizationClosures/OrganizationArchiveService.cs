using DriveOS.Modules.Organizations.Application.OrganizationClosures.Commands;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using Microsoft.Extensions.Logging;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationClosures;

/// <summary>
/// Applies the Organization-context archive boundary. No physical deletion is performed.
/// Documents, finance and audit retention remain owned by their respective bounded contexts.
/// </summary>
internal sealed class OrganizationArchiveService(ILogger<OrganizationArchiveService> logger)
    : IOrganizationArchiveService
{
    public Task ArchiveAsync(OrganizationClosure closure, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(closure);
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation(
            "Organization {OrganizationId} archive policy registered. Disposition={Disposition}, RetentionUntilUtc={RetentionUntilUtc}.",
            closure.OrganizationId,
            closure.DataDisposition,
            closure.RetentionUntilUtc
        );

        return Task.CompletedTask;
    }
}
