using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using Xunit;

namespace DriveOS.UnitTests.Modules.FundingBilling.Payments;

public sealed class PaymentAllocationTests
{
    [Fact]
    public void Paid_payment_can_be_partially_allocated()
    {
        var payment = CreatePaid(100m);
        var result = payment.Allocate(PaymentAllocationId.New(), InvoiceId.New(), null, 40m, new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow);
        result.IsSuccess.Should().BeTrue();
        payment.AllocatedAmount.Should().Be(40m);
        payment.UnallocatedAmount.Should().Be(60m);
    }

    [Fact]
    public void Allocation_cannot_exceed_unallocated_amount()
    {
        var payment = CreatePaid(100m);
        payment.Allocate(PaymentAllocationId.New(), InvoiceId.New(), null, 80m, new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow);
        var result = payment.Allocate(PaymentAllocationId.New(), null, PaymentInstallmentId.New(), 30m, new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FundingBilling.Payment.Allocation.AmountExceeded");
    }

    private static Payment CreatePaid(decimal amount)
    {
        var actor = new UserId(Guid.NewGuid());
        Payment payment = Payment.Create(PaymentId.New(), new OrganizationId(Guid.NewGuid()), new BillingAccountId(Guid.NewGuid()), new PersonId(Guid.NewGuid()), null, amount, "EUR", "Card").Value;
        payment.RecordPaid("test-ref-" + Guid.NewGuid().ToString("N"), actor, DateTimeOffset.UtcNow);
        return payment;
    }
}
