using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.Modules.FundingBilling.Domain.FundingPlans;
using DriveOS.Modules.FundingBilling.Domain.BillingParties;
using DriveOS.Modules.FundingBilling.Domain.TrainingCredits;
using DriveOS.Modules.FundingBilling.Domain.Refunds;
using DriveOS.Modules.FundingBilling.Domain.CreditNotes;
using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
using DriveOS.Modules.FundingBilling.Domain.SupplierPayments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
public sealed class FundingBillingDbContext(DbContextOptions<FundingBillingDbContext> options) : DbContext(options), IFundingBillingUnitOfWork
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)=>
        configurationBuilder.Properties<UserId>().HaveConversion<UserIdConverter>();
    private IDbContextTransaction? currentTransaction;
    public DbSet<BillingAccount> BillingAccounts => Set<BillingAccount>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<PaymentInstallment> PaymentInstallments => Set<PaymentInstallment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<PaymentReminder> PaymentReminders => Set<PaymentReminder>();
    public DbSet<FundingPlan> FundingPlans => Set<FundingPlan>();
    public DbSet<FundingAllocation> FundingAllocations => Set<FundingAllocation>();
    public DbSet<BillingParty> BillingParties => Set<BillingParty>();
    public DbSet<TrainingCreditAccount> TrainingCreditAccounts => Set<TrainingCreditAccount>();
    public DbSet<TrainingCreditMovement> TrainingCreditMovements => Set<TrainingCreditMovement>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<CreditNote> CreditNotes => Set<CreditNote>();
    public DbSet<CreditNoteLine> CreditNoteLines => Set<CreditNoteLine>();
    public DbSet<SupplierInvoice> SupplierInvoices => Set<SupplierInvoice>();
    public DbSet<SupplierPaymentAttempt> SupplierPaymentAttempts => Set<SupplierPaymentAttempt>();
    public DbSet<SupplierPaymentBatch> SupplierPaymentBatches => Set<SupplierPaymentBatch>();
    public DbSet<SupplierPaymentRefund> SupplierPaymentRefunds => Set<SupplierPaymentRefund>();
    public bool HasActiveTransaction => currentTransaction is not null;
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) { if (HasActiveTransaction) throw new InvalidOperationException("A transaction is already active."); currentTransaction = await Database.BeginTransactionAsync(cancellationToken); }
    public Task<int> CommitAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(cancellationToken);
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) { IDbContextTransaction tx = currentTransaction ?? throw new InvalidOperationException("No active transaction exists."); try { await SaveChangesAsync(cancellationToken); await tx.CommitAsync(cancellationToken); } catch { await tx.RollbackAsync(cancellationToken); throw; } finally { await DisposeTransactionAsync(); } }
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default) { IDbContextTransaction tx = currentTransaction ?? throw new InvalidOperationException("No active transaction exists."); try { await tx.RollbackAsync(cancellationToken); ChangeTracker.Clear(); } finally { await DisposeTransactionAsync(); } }
    protected override void OnModelCreating(ModelBuilder modelBuilder) { modelBuilder.HasDefaultSchema(FundingBillingSchema.Name); modelBuilder.ApplyConfigurationsFromAssembly(typeof(FundingBillingDbContext).Assembly); ApplyUserIdConversions(modelBuilder); base.OnModelCreating(modelBuilder); }
    public override void Dispose() { currentTransaction?.Dispose(); currentTransaction = null; base.Dispose(); }
    public override async ValueTask DisposeAsync() { await DisposeTransactionAsync(); await base.DisposeAsync(); }
    private async ValueTask DisposeTransactionAsync() { if (currentTransaction is null) return; await currentTransaction.DisposeAsync(); currentTransaction = null; }
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
