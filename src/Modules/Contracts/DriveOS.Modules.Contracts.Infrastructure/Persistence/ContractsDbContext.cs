using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.Modules.Contracts.Domain.ContractAmendments;
using DriveOS.Modules.Contracts.Domain.SignatureProcesses;
using DriveOS.Modules.Contracts.Domain.ContractDocuments;
using DriveOS.Modules.Contracts.Domain.ProfessionalServiceContracts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace DriveOS.Modules.Contracts.Infrastructure.Persistence;
public sealed class ContractsDbContext(DbContextOptions<ContractsDbContext> options) : DbContext(options), IContractsUnitOfWork
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)=>
        configurationBuilder.Properties<UserId>().HaveConversion<UserIdConverter>();
    private IDbContextTransaction? currentTransaction;
    public DbSet<TrainingContract> TrainingContracts => Set<TrainingContract>();
    public DbSet<TrainingContractVersion> TrainingContractVersions => Set<TrainingContractVersion>();
    public DbSet<SignatureProcess> SignatureProcesses => Set<SignatureProcess>();
    public DbSet<TrainingContractSignatory> TrainingContractSignatories => Set<TrainingContractSignatory>();
    public DbSet<ContractAmendment> ContractAmendments => Set<ContractAmendment>();
    public DbSet<ContractDocument> ContractDocuments => Set<ContractDocument>();
    public DbSet<ContractDocumentVersion> ContractDocumentVersions => Set<ContractDocumentVersion>();
    public DbSet<ProfessionalServiceContract> ProfessionalServiceContracts => Set<ProfessionalServiceContract>();

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
        ApplyUserIdConversions(modelBuilder);
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

    private static void ApplyUserIdConversions(ModelBuilder modelBuilder)
    {
        var required=new ValueConverter<UserId,Guid>(x=>x.Value,x=>new UserId(x));
        var optional=new ValueConverter<UserId?,Guid?>(x=>x.HasValue?x.Value.Value:null,x=>x.HasValue?new UserId(x.Value):null);
        foreach(var property in modelBuilder.Model.GetEntityTypes().SelectMany(x=>x.GetProperties()))
        {
            if(property.GetValueConverter() is not null)continue;
            if(property.ClrType==typeof(UserId))property.SetValueConverter(required);
            else if(property.ClrType==typeof(UserId?))property.SetValueConverter(optional);
        }
    }
    private sealed class UserIdConverter():ValueConverter<UserId,Guid>(x=>x.Value,x=>new UserId(x));
}
