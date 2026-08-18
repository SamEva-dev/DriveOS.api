using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.Modules.Contracts.Domain.ContractAmendments;
using DriveOS.Modules.Contracts.Domain.SignatureProcesses;
using DriveOS.Modules.Contracts.Domain.ContractDocuments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
namespace DriveOS.Modules.Contracts.Infrastructure.Persistence;
public sealed class ContractsDbContext(DbContextOptions<ContractsDbContext> options) : DbContext(options), IContractsUnitOfWork
{
    private IDbContextTransaction? currentTransaction;
    public DbSet<TrainingContract> TrainingContracts => Set<TrainingContract>();
    public DbSet<TrainingContractVersion> TrainingContractVersions => Set<TrainingContractVersion>();
    public DbSet<SignatureProcess> SignatureProcesses => Set<SignatureProcess>();
    public DbSet<TrainingContractSignatory> TrainingContractSignatories => Set<TrainingContractSignatory>();
    public DbSet<ContractAmendment> ContractAmendments => Set<ContractAmendment>();
    public DbSet<ContractDocument> ContractDocuments => Set<ContractDocument>();
    public DbSet<ContractDocumentVersion> ContractDocumentVersions => Set<ContractDocumentVersion>();

    public bool HasActiveTransaction => currentTransaction is not null;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
            throw new InvalidOperationException("A transaction is already active.");

        currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction = currentTransaction
            ?? throw new InvalidOperationException("No active transaction exists.");

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
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction = currentTransaction
            ?? throw new InvalidOperationException("No active transaction exists.");

        try
        {
            await transaction.RollbackAsync(cancellationToken);
            ChangeTracker.Clear();
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(ContractsSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContractsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override void Dispose()
    {
        currentTransaction?.Dispose();
        currentTransaction = null;
        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();
        await base.DisposeAsync();
    }

    private async ValueTask DisposeTransactionAsync()
    {
        if (currentTransaction is null)
            return;

        await currentTransaction.DisposeAsync();
        currentTransaction = null;
    }
}
