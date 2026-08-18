using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.FundingBilling.Payments;

public sealed class PaymentTests
{
    [Fact]
    public void Create_WithValidPersonPayer_CreatesPendingPayment()
    {
        Payment payment = CreatePayment();

        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Amount.Should().Be(250m);
        payment.Currency.Should().Be("EUR");
        payment.PayerPersonId.Should().NotBeNull();
        payment.PayerOrganizationId.Should().BeNull();
    }

    [Fact]
    public void Create_WithTwoPayers_IsRejected()
    {
        var result = Payment.Create(
            PaymentId.New(), new OrganizationId(Guid.NewGuid()), BillingAccountId.New(),
            new PersonId(Guid.NewGuid()), new OrganizationId(Guid.NewGuid()),
            250m, "EUR", "Card");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.InvalidPayer);
    }

    [Fact]
    public void RecordPaid_FromPending_MarksPaymentPaidAndKeepsReference()
    {
        Payment payment = CreatePayment();
        UserId actor = new(Guid.NewGuid());

        var result = payment.RecordPaid("stripe_pi_123", actor, DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Paid);
        payment.ExternalReference.Should().Be("stripe_pi_123");
        payment.PaidAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void MarkFailed_FromProcessing_RecordsFailureReason()
    {
        Payment payment = CreatePayment();
        UserId actor = new(Guid.NewGuid());
        payment.MarkProcessing(actor, DateTimeOffset.UtcNow);

        var result = payment.MarkFailed("Provider refused the payment", actor, DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be("Provider refused the payment");
    }

    [Fact]
    public void Cancel_AfterPaid_IsRejected()
    {
        Payment payment = CreatePayment();
        UserId actor = new(Guid.NewGuid());
        payment.RecordPaid("manual-001", actor, DateTimeOffset.UtcNow);

        var result = payment.Cancel(actor, DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.CancellationNotAllowed);
    }

    private static Payment CreatePayment() => Payment.Create(
        PaymentId.New(),
        new OrganizationId(Guid.NewGuid()),
        BillingAccountId.New(),
        new PersonId(Guid.NewGuid()),
        null,
        250m,
        "eur",
        "Card").Value;
}
