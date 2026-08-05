using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Audit;

public interface IOrganizationActivationReadinessAuditSink
{
    Task WriteAsync(
        OrganizationActivationReadinessAuditEntry entry,
        CancellationToken cancellationToken = default);
}

public sealed record OrganizationActivationReadinessAuditEntry(
    OrganizationId OrganizationId,
    Guid ActorUserId,
    string Action,
    bool IsReady,
    IReadOnlyCollection<string> BlockingRequirementCodes,
    DateTimeOffset OccurredAtUtc);
