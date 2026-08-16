namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;

public interface IOrganizationRepresentativeAccessSynchronizer
{
    Task SynchronizeAsync(
        OrganizationRepresentativeAccessSnapshot representative,
        CancellationToken cancellationToken = default
    );

    Task RevokeAsync(
        OrganizationRepresentativeAccessSnapshot representative,
        string reason,
        CancellationToken cancellationToken = default
    );
}
