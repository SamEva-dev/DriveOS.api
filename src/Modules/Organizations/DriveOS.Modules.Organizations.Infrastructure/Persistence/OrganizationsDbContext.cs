using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence;

public sealed class OrganizationsDbContext :
    DbContext,
    IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public OrganizationsDbContext(
        DbContextOptions<OrganizationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations =>
        Set<Organization>();

    public DbSet<Branch> Branches =>
        Set<Branch>();

    public DbSet<BranchStatusHistoryEntry> BranchStatusHistory =>
        Set<BranchStatusHistoryEntry>();

    public bool HasActiveTransaction =>
        _currentTransaction is not null;

    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
        {
            throw new InvalidOperationException(
                "A transaction is already active.");
        }

        _currentTransaction =
            await Database.BeginTransactionAsync(
                cancellationToken);
    }

    public Task<int> CommitAsync(
        CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction =
            GetActiveTransaction();

        try
        {
            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeCurrentTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction =
            GetActiveTransaction();

        try
        {
            await transaction.RollbackAsync(cancellationToken);
            ChangeTracker.Clear();
        }
        finally
        {
            await DisposeCurrentTransactionAsync();
        }
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(
            OrganizationsSchema.Name);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrganizationsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override void Dispose()
    {
        _currentTransaction?.Dispose();
        _currentTransaction = null;

        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await DisposeCurrentTransactionAsync();
        await base.DisposeAsync();
    }

    private IDbContextTransaction GetActiveTransaction()
    {
        return _currentTransaction
            ?? throw new InvalidOperationException(
                "No active transaction exists.");
    }

    private async ValueTask DisposeCurrentTransactionAsync()
    {
        if (_currentTransaction is null)
        {
            return;
        }

        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }
}
