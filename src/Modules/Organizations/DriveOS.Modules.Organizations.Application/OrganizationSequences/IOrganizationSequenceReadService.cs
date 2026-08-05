using DriveOS.Modules.Organizations.Application.OrganizationSequences.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences;

public interface IOrganizationSequenceReadService
{
    Task<OrganizationSequenceResponse?> GetByIdAsync(
        OrganizationId organizationId,
        OrganizationSequenceId sequenceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationSequenceListItem>> GetListAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        CancellationToken cancellationToken = default);
}
