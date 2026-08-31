using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
using DriveOS.Modules.FundingBilling.Domain.SupplierPayments;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.FundingBilling;

public sealed class SupplierPaymentAttemptTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly OrganizationId Client=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;

    [Fact]
    public void Failed_attempt_preserves_history_and_can_be_retried_with_new_attempt()
    {
        var first=SupplierPaymentAttempt.Schedule(
            new(Guid.NewGuid()),new(Guid.NewGuid()),Client,Guid.NewGuid(),120m,"EUR","SEPA",
            new DateOnly(2026,10,15),"IBAN-REF",Now,Actor).Value;

        first.MarkProcessing(Now,Actor);
        Assert.True(first.MarkFailed("Compte bancaire rejeté",Now,Actor).IsSuccess);
        Assert.Equal(SupplierPaymentAttemptStatus.Failed,first.Status);

        var retry=SupplierPaymentAttempt.Schedule(
            new(Guid.NewGuid()),first.SupplierInvoiceId,Client,first.SupplierOrganizationId,120m,"EUR","SEPA",
            new DateOnly(2026,10,16),"IBAN-REF",Now,Actor);

        Assert.True(retry.IsSuccess);
        Assert.NotEqual(first.Id,retry.Value.Id);
    }

    [Fact]
    public void Paid_attempt_emits_payment_success_event()
    {
        var attempt=SupplierPaymentAttempt.Schedule(
            new(Guid.NewGuid()),new(Guid.NewGuid()),Client,Guid.NewGuid(),120m,"EUR","SEPA",
            new DateOnly(2026,10,15),null,Now,Actor).Value;

        attempt.MarkProcessing(Now,Actor);
        Assert.True(attempt.MarkPaid(attempt.Amount,DateOnly.FromDateTime(Now.UtcDateTime),"BANK-2026-0001",Now,Actor).IsSuccess);
        Assert.Equal(SupplierPaymentAttemptStatus.Paid,attempt.Status);
        Assert.Contains(attempt.DomainEvents,e=>e.GetType().Name=="SupplierPaymentSucceededDomainEvent");
    }

    [Fact]
    public void Failed_supplier_payment_reopens_invoice_for_retry()
    {
        var invoice=SupplierInvoice.Receive(
            new(Guid.NewGuid()),Client,Guid.NewGuid(),SupplierInvoiceSourceType.ProfessionalMarketplace,
            Guid.NewGuid(),Guid.NewGuid(),"F-2026-001",
            new DateOnly(2026,10,1),new DateOnly(2026,10,31),"EUR",100m,20m,"FreelanceIssued",Now,Actor).Value;

        invoice.MarkMatched(Actor,Now);
        invoice.ApproveOperational(Actor,Now);
        invoice.ApproveFinancial(Actor,Now);
        invoice.SchedulePayment(invoice.RemainingAmount,Actor,Now);

        Assert.True(invoice.ReopenAfterFailedPayment("virement rejeté",Now,Actor).IsSuccess);
        Assert.Equal(SupplierInvoiceStatus.Approved,invoice.Status);
    }
}
