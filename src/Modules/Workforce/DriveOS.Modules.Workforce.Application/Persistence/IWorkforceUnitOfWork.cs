namespace DriveOS.Modules.Workforce.Application.Persistence;
public interface IWorkforceUnitOfWork { Task<int> CommitAsync(CancellationToken cancellationToken = default); }
