using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness;

public interface IOrganizationActivationReadinessDataSource
{
    Task<bool> HasActiveLegalProfileAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveOwnerAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<bool> HasActivePrimaryOwnerAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveSubscriptionAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<bool> HasOperationalSettingsAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<BranchId?> GetPrimaryBranchIdAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveBranchManagerAsync(OrganizationId organizationId, BranchId branchId, CancellationToken cancellationToken = default);
}
