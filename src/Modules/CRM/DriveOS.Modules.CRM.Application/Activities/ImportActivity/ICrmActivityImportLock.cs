using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Activities.ImportActivity;

public interface ICrmActivityImportLock
{
    Task AcquireAsync(
        OrganizationId organizationId,
        string idempotencyKey,
        CancellationToken cancellationToken
    );
}
