using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences;

/// <summary>
/// Public inter-module contract used to reserve an organization or branch-scoped
/// business reference without exposing the Organizations aggregate or DbContext.
/// </summary>
public interface IOrganizationSequenceNumberGenerator
{
    Task<Result<string>> ReserveNextAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        string code,
        CancellationToken cancellationToken = default
    );
}
