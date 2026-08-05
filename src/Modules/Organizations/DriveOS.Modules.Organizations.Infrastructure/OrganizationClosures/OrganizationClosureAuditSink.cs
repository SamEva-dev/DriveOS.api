using DriveOS.Modules.Organizations.Application.OrganizationClosures.Commands;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.Extensions.Logging;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationClosures;

internal sealed class OrganizationClosureAuditSink(
    ILogger<OrganizationClosureAuditSink> logger)
    : IOrganizationClosureAuditSink
{
    public Task WriteAsync(
        string action,
        OrganizationId organizationId,
        OrganizationClosureId closureId,
        UserId actorUserId,
        IReadOnlyDictionary<string, object?> data,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation(
            "Organization closure audit: Action={Action}, OrganizationId={OrganizationId}, ClosureId={ClosureId}, ActorUserId={ActorUserId}, Data={@Data}",
            action,
            organizationId,
            closureId,
            actorUserId,
            data);

        return Task.CompletedTask;
    }
}
