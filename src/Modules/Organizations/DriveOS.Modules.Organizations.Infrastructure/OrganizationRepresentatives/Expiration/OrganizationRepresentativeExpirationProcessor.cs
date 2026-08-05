using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Expiration;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationRepresentatives.Expiration;

internal sealed class OrganizationRepresentativeExpirationProcessor(
    OrganizationsDbContext dbContext,
    OrganizationRepresentativeAccessSynchronizationService accessSynchronizationService,
    IOptions<OrganizationRepresentativeExpirationOptions> options)
    : IOrganizationRepresentativeExpirationProcessor
{
    public async Task<int> ProcessAsync(DateOnly today, int batchSize, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(options.Value.SystemUserId, out Guid rawSystemUserId) || rawSystemUserId == Guid.Empty)
            return 0;

        UserId systemUserId = new(rawSystemUserId);
        List<OrganizationRepresentative> candidates = await dbContext.OrganizationRepresentatives
            .Where(x =>
                x.RepresentativeType != OrganizationRepresentativeType.Owner &&
                x.EffectiveTo.HasValue &&
                x.EffectiveTo.Value < today &&
                (x.Status == OrganizationRepresentativeStatus.Active ||
                 x.Status == OrganizationRepresentativeStatus.Suspended))
            .OrderBy(x => x.EffectiveTo)
            .Take(Math.Clamp(batchSize, 1, 1000))
            .ToListAsync(cancellationToken);

        var ended = new List<OrganizationRepresentative>(candidates.Count);
        foreach (OrganizationRepresentative representative in candidates)
        {
            var result = representative.End(
                representative.EffectiveTo!.Value,
                "Automatic expiration of the representative mandate.",
                systemUserId,
                isLastActiveOwner: false);

            if (result.IsSuccess)
                ended.Add(representative);
        }

        if (ended.Count == 0)
            return 0;

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (OrganizationRepresentative representative in ended)
            await accessSynchronizationService.SynchronizeAsync(representative, cancellationToken);

        return ended.Count;
    }
}
