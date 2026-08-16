namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Expiration;

public interface IOrganizationRepresentativeExpirationProcessor
{
    Task<int> ProcessAsync(
        DateOnly today,
        int batchSize,
        CancellationToken cancellationToken = default
    );
}
