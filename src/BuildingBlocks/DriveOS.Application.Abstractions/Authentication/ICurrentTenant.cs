using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Application.Abstractions.Authentication;

public interface ICurrentTenant
{
    bool HasTenant { get; }

    OrganizationId? OrganizationId { get; }

    BranchId? BranchId { get; }
}
