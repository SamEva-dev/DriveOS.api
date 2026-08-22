namespace DriveOS.Modules.FleetResources.Application.Persistence;

public interface IFleetResourcesUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
