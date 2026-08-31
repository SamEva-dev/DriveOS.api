namespace DriveOS.Modules.CommunicationEngagement.Application.Persistence;
public interface ICommunicationEngagementUnitOfWork { Task<int> CommitAsync(CancellationToken cancellationToken=default); }
