using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.Commands;

public interface IOrganizationArchiveService
{
    Task ArchiveAsync(OrganizationClosure closure, CancellationToken cancellationToken);
}

public interface IOrganizationAnonymizationService
{
    Task<bool> HasIrreversibleAnonymizationStartedAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken
    );
    Task AnonymizeAsync(
        OrganizationId organizationId,
        DateTimeOffset dueAtUtc,
        CancellationToken cancellationToken
    );
}

public interface IOrganizationClosureAuditSink
{
    Task WriteAsync(
        string action,
        OrganizationId organizationId,
        OrganizationClosureId closureId,
        UserId actorUserId,
        IReadOnlyDictionary<string, object?> data,
        CancellationToken cancellationToken
    );
}
