using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Audit;
using Microsoft.Extensions.Logging;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationActivationReadiness.Audit;

internal sealed class LoggerOrganizationActivationReadinessAuditSink(
    ILogger<LoggerOrganizationActivationReadinessAuditSink> logger
) : IOrganizationActivationReadinessAuditSink
{
    public Task WriteAsync(
        OrganizationActivationReadinessAuditEntry entry,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "Organization activation readiness evaluated. OrganizationId={OrganizationId} ActorUserId={ActorUserId} Action={Action} IsReady={IsReady} BlockingRequirements={BlockingRequirements} OccurredAtUtc={OccurredAtUtc}",
            entry.OrganizationId.Value,
            entry.ActorUserId,
            entry.Action,
            entry.IsReady,
            entry.BlockingRequirementCodes,
            entry.OccurredAtUtc
        );

        return Task.CompletedTask;
    }
}
