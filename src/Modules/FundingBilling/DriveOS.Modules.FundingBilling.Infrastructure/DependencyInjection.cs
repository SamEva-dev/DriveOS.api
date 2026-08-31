using DriveOS.Modules.FundingBilling.Application.BillingAccounts.Read;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Application.Invoices.Read;
using DriveOS.Modules.FundingBilling.Application.Installments.Read;
using DriveOS.Modules.FundingBilling.Application.Payments.Read;
using DriveOS.Modules.FundingBilling.Application.Collections.Read;
using DriveOS.Modules.FundingBilling.Application.FundingPlans.Read;
using DriveOS.Modules.FundingBilling.Application.BillingParties.Read;
using DriveOS.Modules.FundingBilling.Application.TrainingCredits.Read;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.Modules.FundingBilling.Domain.FundingPlans;
using DriveOS.Modules.FundingBilling.Domain.BillingParties;
using DriveOS.Modules.FundingBilling.Domain.TrainingCredits;
using DriveOS.Modules.FundingBilling.Domain.Refunds;
using DriveOS.Modules.FundingBilling.Application.Refunds.Read;
using DriveOS.Modules.FundingBilling.Domain.CreditNotes;
using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
using DriveOS.Modules.FundingBilling.Domain.SupplierPayments;
using DriveOS.Modules.FundingBilling.Application.SupplierPayments;
using DriveOS.Modules.FundingBilling.Application.SupplierInvoices;
using DriveOS.Modules.FundingBilling.Application.CreditNotes.Read;
using DriveOS.Modules.FundingBilling.Application.StudentFinance.Read;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Interceptors;
using DriveOS.Modules.FundingBilling.Infrastructure.Auditing;
using DriveOS.Modules.FundingBilling.Application.Auditing;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;
using DriveOS.Modules.FundingBilling.Infrastructure.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace DriveOS.Modules.FundingBilling.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddFundingBillingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string cs = configuration.GetConnectionString("DriveOS") ?? throw new InvalidOperationException("The DriveOS database connection string is missing.");
        services.AddScoped<FundingBillingAuditInterceptor>();
        services.AddDbContext<FundingBillingDbContext>((provider, options) =>
        {
            options.UseNpgsql(cs, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", FundingBillingSchema.Name));
            options.AddInterceptors(provider.GetRequiredService<FundingBillingAuditInterceptor>());
        });
        services.AddScoped<IFundingBillingUnitOfWork>(sp => sp.GetRequiredService<FundingBillingDbContext>());
        services.AddScoped<IStudentBillingAccountRepository, StudentBillingAccountRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentInstallmentRepository, PaymentInstallmentRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentReminderRepository, PaymentReminderRepository>();
        services.AddScoped<IFundingPlanRepository, FundingPlanRepository>();
        services.AddScoped<IBillingPartyRepository, BillingPartyRepository>();
        services.AddScoped<ITrainingCreditAccountRepository, TrainingCreditAccountRepository>();
        services.AddScoped<IRefundRepository, RefundRepository>();
        services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();
        services.AddScoped<ISupplierInvoiceRepository, SupplierInvoiceRepository>();
        services.AddScoped<ISupplierPaymentAttemptRepository, SupplierPaymentAttemptRepository>();
        services.AddScoped<ISupplierPaymentBatchRepository, SupplierPaymentBatchRepository>();
        services.AddScoped<ISupplierPaymentRefundRepository, SupplierPaymentRefundRepository>();
        services.AddScoped<IBillingAccountReadService, BillingAccountReadService>();
        services.AddScoped<IInvoiceReadService, InvoiceReadService>();
        services.AddScoped<IPaymentInstallmentReadService, PaymentInstallmentReadService>();
        services.AddScoped<IPaymentReadService, PaymentReadService>();
        services.AddScoped<ICollectionReadService, CollectionReadService>();
        services.AddScoped<IFundingPlanReadService, FundingPlanReadService>();
        services.AddScoped<IBillingPartyReadService, BillingPartyReadService>();
        services.AddScoped<ITrainingCreditAccountReadService, TrainingCreditAccountReadService>();
        services.AddScoped<IRefundReadService, RefundReadService>();
        services.AddScoped<ICreditNoteReadService, CreditNoteReadService>();
        services.AddScoped<ISupplierInvoiceReadService, SupplierInvoiceReadService>();
        services.AddScoped<ISupplierPaymentTimelineReadService, SupplierPaymentTimelineReadService>();
        services.AddScoped<ISupplierSettlementOverdueAutomation, SupplierSettlementOverdueAutomation>();
        services.AddScoped<IStudentFinancialOverviewReadService, StudentFinancialOverviewReadService>();
        services.AddScoped<IFinancialAuditReadService, FinancialAuditReadService>();
        return services;
    }
}
