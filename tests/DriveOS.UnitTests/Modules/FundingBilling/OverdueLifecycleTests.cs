using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.FundingBilling;

public sealed class OverdueLifecycleTests
{
    [Fact]
    public void Invoice_can_be_marked_overdue_only_after_due_date()
    {
        var invoice = Invoice.CreateDraft(InvoiceId.New(), OrganizationId.New(), BillingAccountId.New(), PersonId.New(), "EUR").Value;
        invoice.AddLine(InvoiceLineId.New(), "Lesson", 1m, "hour", 100m, 0m, 20m);
        invoice.Issue("INV-001", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10), UserId.New(), DateTimeOffset.UtcNow);

        Assert.True(invoice.MarkOverdue(new DateOnly(2026, 8, 11), DateTimeOffset.UtcNow).IsSuccess);
        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
        Assert.NotNull(invoice.OverdueAtUtc);
    }

    [Fact]
    public void Partial_payment_keeps_overdue_invoice_overdue()
    {
        var invoice = Invoice.CreateDraft(InvoiceId.New(), OrganizationId.New(), BillingAccountId.New(), PersonId.New(), "EUR").Value;
        invoice.AddLine(InvoiceLineId.New(), "Lesson", 1m, "hour", 100m, 0m, 0m);
        var actor = UserId.New();
        invoice.Issue("INV-002", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10), actor, DateTimeOffset.UtcNow);
        invoice.MarkOverdue(new DateOnly(2026, 8, 11), DateTimeOffset.UtcNow);

        Assert.True(invoice.RecordPaymentAllocation(20m, "EUR", actor, DateTimeOffset.UtcNow).IsSuccess);
        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
    }

    [Fact]
    public void Installment_can_be_marked_overdue_and_stays_overdue_after_partial_payment()
    {
        var actor = UserId.New();
        var installment = PaymentInstallment.Create(PaymentInstallmentId.New(), OrganizationId.New(), BillingAccountId.New(), new DateOnly(2026, 8, 10), 100m, "EUR").Value;

        Assert.True(installment.MarkOverdue(new DateOnly(2026, 8, 11), DateTimeOffset.UtcNow).IsSuccess);
        Assert.True(installment.RecordPaymentAllocation(20m, "EUR", actor, DateTimeOffset.UtcNow).IsSuccess);
        Assert.Equal(PaymentInstallmentStatus.Overdue, installment.Status);
    }
}
