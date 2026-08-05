using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationRepresentatives.AccessSynchronization;

internal sealed class NoOpOrganizationRepresentativeAccessSynchronizer
    : IOrganizationRepresentativeAccessSynchronizer
{
    public Task SynchronizeAsync(OrganizationRepresentativeAccessSnapshot representative, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RevokeAsync(OrganizationRepresentativeAccessSnapshot representative, string reason, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
