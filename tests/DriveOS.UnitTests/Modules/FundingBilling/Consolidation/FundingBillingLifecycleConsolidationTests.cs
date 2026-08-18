using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.FundingBilling.Consolidation;

public sealed class FundingBillingLifecycleConsolidationTests
{
    [Fact]
    public void FullyAllocatedPayment_ShouldLeaveNoUnallocatedAmount()
    {
        OrganizationId organizationId = OrganizationId.New();
        BillingAccountId billingAccountId = BillingAccountId.New();
        PersonId studentId = PersonId.New();
        UserId actor = UserId.New();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Payment payment = Payment.Create(
            PaymentId.New(),
            organizationId,
            billingAccountId,
            studentId,
            null,
            100m,
            "EUR",
            "Card",
            null).Value;

        payment.RecordPaid("provider-1", actor, now).IsSuccess.Should().BeTrue();
        payment.Allocate(PaymentAllocationId.New(), InvoiceId.New(), null, 60m, actor, now).IsSuccess.Should().BeTrue();
        payment.Allocate(PaymentAllocationId.New(), InvoiceId.New(), null, 40m, actor, now).IsSuccess.Should().BeTrue();

        payment.UnallocatedAmount.Should().Be(0m);
        payment.Allocations.Sum(x => x.Amount).Should().Be(100m);
    }

    [Fact]
    public void ClosedBillingAccount_ShouldRejectNewFinancialActivity()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();
        UserId actor = UserId.New();
        BillingAccount account = BillingAccount.CreateForStudent(
            BillingAccountId.New(),
            organizationId,
            studentId,
            "EUR").Value;

        account.Close("Training completed", actor, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();

        account.RecordInvoiceIssued(100m, "EUR", actor, DateTimeOffset.UtcNow)
            .IsFailure.Should().BeTrue();
    }
}
