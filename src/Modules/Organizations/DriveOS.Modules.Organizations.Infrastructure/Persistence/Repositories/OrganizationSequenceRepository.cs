using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Repositories;

internal sealed class OrganizationSequenceRepository(OrganizationsDbContext dbContext)
    : IOrganizationSequenceRepository
{
    public Task<OrganizationSequence?> GetForUpdateAsync(
        OrganizationSequenceId sequenceId,
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    ) =>
        dbContext.OrganizationSequences.SingleOrDefaultAsync(
            sequence => sequence.Id == sequenceId && sequence.OrganizationId == organizationId,
            cancellationToken
        );

    public Task<OrganizationSequence?> GetByCodeForUpdateAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        string code,
        CancellationToken cancellationToken = default
    )
    {
        string normalizedCode = code.Trim().ToUpperInvariant();

        return dbContext.OrganizationSequences.SingleOrDefaultAsync(
            sequence =>
                sequence.OrganizationId == organizationId
                && sequence.BranchId == branchId
                && sequence.Code == normalizedCode,
            cancellationToken
        );
    }

    public Task<bool> ExistsAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        string code,
        CancellationToken cancellationToken = default
    )
    {
        string normalizedCode = code.Trim().ToUpperInvariant();

        return dbContext
            .OrganizationSequences.AsNoTracking()
            .AnyAsync(
                sequence =>
                    sequence.OrganizationId == organizationId
                    && sequence.BranchId == branchId
                    && sequence.Code == normalizedCode,
                cancellationToken
            );
    }

    public async Task AddAsync(
        OrganizationSequence sequence,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(sequence);
        await dbContext.OrganizationSequences.AddAsync(sequence, cancellationToken);
    }
}
