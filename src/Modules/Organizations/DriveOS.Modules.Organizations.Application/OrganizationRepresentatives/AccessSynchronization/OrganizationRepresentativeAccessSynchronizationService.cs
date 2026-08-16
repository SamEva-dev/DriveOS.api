using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;

public sealed class OrganizationRepresentativeAccessSynchronizationService(
    IOrganizationRepresentativeAccessSynchronizer synchronizer
)
{
    public Task SynchronizeAsync(
        OrganizationRepresentative representative,
        CancellationToken cancellationToken = default
    )
    {
        if (representative.UserId is null)
            return Task.CompletedTask;

        OrganizationRepresentativeAccessSnapshot snapshot = CreateSnapshot(representative);

        return representative.Status is OrganizationRepresentativeStatus.Active
            ? synchronizer.SynchronizeAsync(snapshot, cancellationToken)
            : synchronizer.RevokeAsync(
                snapshot,
                $"Representative status is {representative.Status}.",
                cancellationToken
            );
    }

    private static OrganizationRepresentativeAccessSnapshot CreateSnapshot(
        OrganizationRepresentative representative
    ) =>
        new(
            representative.Id,
            representative.OrganizationId,
            representative.PersonId,
            representative.UserId!.Value,
            representative.RepresentativeType,
            representative.AuthorityScope.Value,
            representative.IsPrimaryOwner,
            representative.EffectiveFrom,
            representative.EffectiveTo,
            representative.Status,
            representative.Revision
        );
}
