using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.SharedKernel.Tenancy;

public interface ITenantEntity
{
    OrganizationId OrganizationId { get; }
}
