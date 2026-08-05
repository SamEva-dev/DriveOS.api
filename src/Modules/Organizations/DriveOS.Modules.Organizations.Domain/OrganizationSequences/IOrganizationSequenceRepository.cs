using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSequences;

public interface IOrganizationSequenceRepository
{
    Task<OrganizationSequence?> GetForUpdateAsync(
        OrganizationSequenceId sequenceId,
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task<OrganizationSequence?> GetByCodeForUpdateAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        string code,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        string code,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        OrganizationSequence sequence,
        CancellationToken cancellationToken = default);
}
