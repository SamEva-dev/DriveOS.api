using System.Linq.Expressions;

namespace DriveOS.SharedKernel.Persistence;

public interface IRepository<TEntity, in TId>
    where TEntity : class
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(
        TId id,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TEntity>> GetAllAsync(
        bool asNoTracking = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
