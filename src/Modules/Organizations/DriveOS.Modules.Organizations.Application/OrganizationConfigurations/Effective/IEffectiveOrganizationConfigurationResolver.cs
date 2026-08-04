using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Effective;

public interface IEffectiveOrganizationConfigurationResolver
{
    Task<EffectiveOrganizationConfiguration?> ResolveCurrentAsync(
        OrganizationId organizationId,
        BranchId? branchId = null,
        CancellationToken cancellationToken = default);

    Task<EffectiveOrganizationConfiguration?> ResolveAtAsync(
        OrganizationId organizationId,
        DateTimeOffset instantUtc,
        BranchId? branchId = null,
        CancellationToken cancellationToken = default);
}

public interface IOrganizationConfigurationCacheInvalidator
{
    void Invalidate(OrganizationId organizationId);
}
