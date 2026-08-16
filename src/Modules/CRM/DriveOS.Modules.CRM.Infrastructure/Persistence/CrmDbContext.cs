using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Conversions;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.Modules.CRM.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence;

public sealed class CrmDbContext : DbContext, ICrmUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public CrmDbContext(DbContextOptions<CrmDbContext> options)
        : base(options) { }

    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadConversion> LeadConversions => Set<LeadConversion>();
    public DbSet<CrmTask> Tasks => Set<CrmTask>();
    public DbSet<CrmActivity> Activities => Set<CrmActivity>();
    public DbSet<AssessmentAppointment> AssessmentAppointments => Set<AssessmentAppointment>();
    public DbSet<AssessmentSession> AssessmentSessions => Set<AssessmentSession>();
    public DbSet<AssessmentSessionRevision> AssessmentSessionRevisions =>
        Set<AssessmentSessionRevision>();
    public DbSet<CommercialOffer> CommercialOffers => Set<CommercialOffer>();
    public DbSet<CommercialOfferLine> CommercialOfferLines => Set<CommercialOfferLine>();
    public DbSet<SavedLeadView> SavedLeadViews => Set<SavedLeadView>();

    public bool HasActiveTransaction => _currentTransaction is not null;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
        {
            throw new InvalidOperationException("A transaction is already active.");
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction = GetActiveTransaction();

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

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction = GetActiveTransaction();

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(CrmSchema.Name);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);

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
            ?? throw new InvalidOperationException("No active transaction exists.");
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
