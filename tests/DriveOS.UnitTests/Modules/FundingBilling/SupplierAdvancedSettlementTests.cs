using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
using DriveOS.Modules.FundingBilling.Domain.SupplierPayments;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.FundingBilling;

public sealed class SupplierAdvancedSettlementTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly OrganizationId Client=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;

    private static SupplierInvoice ApprovedInvoice()
    {
        var invoice=SupplierInvoice.Receive(
            new(Guid.NewGuid()),Client,Guid.NewGuid(),SupplierInvoiceSourceType.ProfessionalMarketplace,
            Guid.NewGuid(),Guid.NewGuid(),"F-2026-100",
            new DateOnly(2026,8,1),new DateOnly(2026,8,31),
            "EUR",100m,20m,"FreelanceIssued",Now,Actor).Value;
        invoice.MarkMatched(Actor,Now);
        invoice.ApproveOperational(Actor,Now);
        invoice.ApproveFinancial(Actor,Now);
        return invoice;
    }

    [Fact]
    public void Partial_payment_keeps_remaining_amount()
    {
        var invoice=ApprovedInvoice();

        Assert.True(invoice.SchedulePayment(50m,Actor,Now).IsSuccess);
        Assert.True(invoice.ApplySettledPayment(50m,Now,Actor).IsSuccess);

        Assert.Equal(50m,invoice.PaidAmount);
        Assert.Equal(70m,invoice.RemainingAmount);
        Assert.Equal(SupplierInvoiceSettlementStatus.PartiallyPaid,invoice.SettlementStatus);
        Assert.Equal(SupplierInvoiceStatus.Approved,invoice.Status);
    }

    [Fact]
    public void Payment_cannot_exceed_remaining_invoice_balance()
    {
        var invoice=ApprovedInvoice();

        Assert.True(invoice.SchedulePayment(120m,Actor,Now).IsSuccess);
        var result=invoice.ApplySettledPayment(121m,Now,Actor);

        Assert.True(result.IsFailure);
        Assert.Equal(0m,invoice.PaidAmount);
        Assert.Equal(120m,invoice.RemainingAmount);
    }

    [Fact]
    public void Reconciliation_detects_underpayment()
    {
        var attempt=SupplierPaymentAttempt.Schedule(
            new(Guid.NewGuid()),new(Guid.NewGuid()),Client,Guid.NewGuid(),
            100m,"EUR","SEPA",new DateOnly(2026,8,31),"BANK-1",Now,Actor).Value;

        Assert.True(attempt.MarkPaid(95m,new DateOnly(2026,8,31),"PROVIDER-1",Now,Actor).IsSuccess);
        Assert.Equal(SupplierPaymentReconciliationStatus.Underpayment,attempt.ReconciliationStatus);
        Assert.Equal(-5m,attempt.ReconciliationDifference);
    }

    [Fact]
    public void Refund_reopens_remaining_balance()
    {
        var invoice=ApprovedInvoice();
        invoice.SchedulePayment(120m,Actor,Now);
        invoice.ApplySettledPayment(120m,Now,Actor);

        Assert.Equal(SupplierInvoiceSettlementStatus.Paid,invoice.SettlementStatus);
        Assert.True(invoice.RecordRefund(20m,"Correction bancaire",Now,Actor).IsSuccess);

        Assert.Equal(100m,invoice.NetPaidAmount);
        Assert.Equal(20m,invoice.RemainingAmount);
        Assert.Equal(SupplierInvoiceSettlementStatus.PartiallyPaid,invoice.SettlementStatus);
    }

    [Fact]
    public void Invoice_becomes_overdue_only_when_balance_remains()
    {
        var invoice=ApprovedInvoice();

        Assert.True(invoice.MarkOverdue(new DateOnly(2026,9,2),Now).IsSuccess);
        Assert.Equal(SupplierInvoiceSettlementStatus.Overdue,invoice.SettlementStatus);
    }
}
