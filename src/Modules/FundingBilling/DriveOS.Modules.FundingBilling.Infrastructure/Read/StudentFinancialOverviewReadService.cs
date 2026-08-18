using DriveOS.Modules.FundingBilling.Application.StudentFinance.Read;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.Modules.FundingBilling.Domain.CreditNotes;
using DriveOS.Modules.FundingBilling.Domain.FundingPlans;
using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.Modules.FundingBilling.Domain.Refunds;
using DriveOS.Modules.FundingBilling.Domain.TrainingCredits;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;

public sealed class StudentFinancialOverviewReadService(FundingBillingDbContext db) : IStudentFinancialOverviewReadService
{
    public async Task<StudentFinancialOverviewResponse?> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        DateOnly businessDate,
        CancellationToken cancellationToken = default)
    {
        BillingAccount? account = await db.BillingAccounts
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.StudentId == studentId, cancellationToken);

        if (account is null)
            return null;

        BillingAccountId billingAccountId = account.Id;

        Invoice[] invoices = await db.Invoices
            .AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId)
            .ToArrayAsync(cancellationToken);

        PaymentInstallment[] installments = await db.PaymentInstallments
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId)
            .ToArrayAsync(cancellationToken);

        Payment[] payments = await db.Payments
            .AsNoTracking()
            .Include(x => x.Allocations)
            .Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId)
            .ToArrayAsync(cancellationToken);

        FundingPlan[] fundingPlans = await db.FundingPlans
            .AsNoTracking()
            .Include(x => x.Allocations)
            .Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId)
            .ToArrayAsync(cancellationToken);

        var billingParties = await db.BillingParties
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId)
            .OrderBy(x => x.Priority)
            .ToArrayAsync(cancellationToken);

        TrainingCreditAccount[] credits = await db.TrainingCreditAccounts
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId)
            .ToArrayAsync(cancellationToken);

        Refund[] refunds = await db.Refunds
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId)
            .ToArrayAsync(cancellationToken);

        CreditNote[] creditNotes = await db.CreditNotes
            .AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId)
            .ToArrayAsync(cancellationToken);

        int pendingReminderCount = await db.PaymentReminders
            .AsNoTracking()
            .CountAsync(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId && x.Status == PaymentReminderStatus.Pending, cancellationToken);

        decimal invoiceOverdue = invoices.Where(x => x.Status == InvoiceStatus.Overdue).Sum(x => x.RemainingAmount);
        decimal installmentOverdue = installments.Where(x => x.Status == PaymentInstallmentStatus.Overdue).Sum(x => x.RemainingAmount);
        // Installments may represent the same receivable as invoices; do not blindly sum both dimensions.
        decimal consolidatedOverdueAmount = decimal.Max(invoiceOverdue, installmentOverdue);

        decimal totalRefunded = refunds.Where(x => x.Status == RefundStatus.Completed).Sum(x => x.Amount);
        decimal totalCredited = creditNotes.Where(x => x.Status == CreditNoteStatus.Issued).Sum(x => x.TotalAmount);
        decimal unallocatedPayments = payments
            .Where(x => x.Status is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded)
            .Sum(x => x.UnallocatedAmount);
        decimal approvedFunding = fundingPlans.Sum(x => x.ApprovedFundingAmount);
        decimal plannedFunding = fundingPlans
            .Where(x => x.Status is not (FundingPlanStatus.Rejected or FundingPlanStatus.Cancelled))
            .Sum(x => x.StudentContribution + x.RequestedFundingAmount);
        decimal availableCredits = credits.Where(x => x.Status == TrainingCreditAccountStatus.Active).Sum(x => x.QuantityAvailable);

        PaymentInstallment? nextInstallment = installments
            .Where(x => x.RemainingAmount > 0m && x.Status is not (PaymentInstallmentStatus.Cancelled or PaymentInstallmentStatus.Waived or PaymentInstallmentStatus.Paid))
            .OrderBy(x => x.DueDate)
            .FirstOrDefault();

        int expiringCredits = credits.Count(x =>
            x.Status == TrainingCreditAccountStatus.Active &&
            x.ExpirationDate.HasValue &&
            x.ExpirationDate.Value >= businessDate &&
            x.ExpirationDate.Value <= businessDate.AddDays(30) &&
            x.QuantityAvailable > 0m);

        var alerts = new StudentFinancialAlertsResponse(
            invoices.Count(x => x.Status == InvoiceStatus.Overdue && x.RemainingAmount > 0m),
            installments.Count(x => x.Status == PaymentInstallmentStatus.Overdue && x.RemainingAmount > 0m),
            pendingReminderCount,
            payments.Count(x => x.Status == PaymentStatus.Failed),
            fundingPlans.Count(x => x.Status is FundingPlanStatus.PendingApproval or FundingPlanStatus.PartiallyApproved),
            expiringCredits,
            refunds.Count(x => x.Status is RefundStatus.Requested or RefundStatus.Approved or RefundStatus.Processing),
            account.Status is BillingAccountStatus.Restricted or BillingAccountStatus.Suspended || consolidatedOverdueAmount > 0m);

        return new StudentFinancialOverviewResponse(
            account.Id.Value,
            account.StudentId.Value,
            account.Currency,
            account.Status.ToString(),
            new StudentFinancialTotalsResponse(
                account.TotalInvoiced,
                account.TotalPaid,
                totalRefunded,
                totalCredited,
                account.CreditBalance,
                account.OutstandingBalance,
                consolidatedOverdueAmount,
                unallocatedPayments,
                approvedFunding,
                plannedFunding,
                availableCredits),
            alerts,
            nextInstallment is null
                ? null
                : new StudentFinancialNextInstallmentResponse(nextInstallment.Id.Value, nextInstallment.DueDate, nextInstallment.ExpectedAmount, nextInstallment.PaidAmount, nextInstallment.RemainingAmount, nextInstallment.Status.ToString()),
            invoices
                .OrderByDescending(x => x.IssueDate)
                .ThenByDescending(x => x.CreatedAtUtc)
                .Take(5)
                .Select(x => new StudentFinancialInvoiceSummaryResponse(x.Id.Value, x.InvoiceNumber, x.IssueDate, x.DueDate, x.Status.ToString(), x.TotalAmount, x.PaidAmount, x.CreditedAmount, x.RemainingAmount))
                .ToArray(),
            payments
                .OrderByDescending(x => x.PaidAtUtc ?? x.CreatedAtUtc)
                .Take(5)
                .Select(x => new StudentFinancialPaymentSummaryResponse(x.Id.Value, x.Amount, x.AllocatedAmount, x.UnallocatedAmount, x.RefundedAmount, x.Status.ToString(), x.PaymentMethod, x.PaidAtUtc))
                .ToArray(),
            fundingPlans
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new StudentFinancialFundingPlanSummaryResponse(x.Id.Value, x.ContractId, x.TotalCost, x.StudentContribution, x.RequestedFundingAmount, x.ApprovedFundingAmount, x.Status.ToString()))
                .ToArray(),
            billingParties
                .Select(x => new StudentFinancialBillingPartySummaryResponse(x.Id.Value, x.PersonId?.Value, x.PartyOrganizationId?.Value, x.Role.ToString(), x.MaximumAmount, x.Priority, x.IsPrimary, x.Status.ToString(), x.EffectiveFrom, x.EffectiveTo))
                .ToArray(),
            credits
                .Select(x => new StudentFinancialCreditSummaryResponse(x.Id.Value, x.CreditType, x.QuantityPurchased, x.QuantityReserved, x.QuantityConsumed, x.Adjustments, x.QuantityAvailable, x.ExpirationDate, x.Status.ToString()))
                .ToArray(),
            refunds
                .OrderByDescending(x => x.RequestedAtUtc)
                .Take(5)
                .Select(x => new StudentFinancialRefundSummaryResponse(x.Id.Value, x.PaymentId.Value, x.Amount, x.Status.ToString(), x.RequestedAtUtc, x.CompletedAtUtc))
                .ToArray(),
            creditNotes
                .OrderByDescending(x => x.IssueDate)
                .ThenByDescending(x => x.CreatedAtUtc)
                .Take(5)
                .Select(x => new StudentFinancialCreditNoteSummaryResponse(x.Id.Value, x.InvoiceId.Value, x.CreditNoteNumber, x.IssueDate, x.TotalAmount, x.Status.ToString()))
                .ToArray());
    }
}
