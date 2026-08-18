using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts.Events;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.FundingBilling.BillingAccounts;

public sealed class BillingAccountTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.NewGuid());
    private static readonly PersonId StudentId = new(Guid.NewGuid());
    private static readonly UserId ActorUserId = new(Guid.NewGuid());

    [Fact]
    public void CreateForStudent_WithValidData_CreatesOpenAccountAndNormalizesCurrency()
    {
        var result = BillingAccount.CreateForStudent(
            BillingAccountId.New(),
            OrganizationId,
            StudentId,
            " eur ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(BillingAccountStatus.Open);
        result.Value.Currency.Should().Be("EUR");
        result.Value.TotalInvoiced.Should().Be(0m);
        result.Value.TotalPaid.Should().Be(0m);
        result.Value.CreditBalance.Should().Be(0m);
        result.Value.OutstandingBalance.Should().Be(0m);
        result.Value.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<BillingAccountCreatedDomainEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("12A")]
    public void CreateForStudent_WithInvalidCurrency_Fails(string currency)
    {
        var result = BillingAccount.CreateForStudent(
            BillingAccountId.New(),
            OrganizationId,
            StudentId,
            currency);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BillingAccountErrors.InvalidCurrency);
    }

    [Fact]
    public void Restrict_ThenReactivate_RestoresOpenStatus()
    {
        BillingAccount account = CreateAccount();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        account.Restrict("Manual financial review", ActorUserId, now).IsSuccess.Should().BeTrue();
        account.Status.Should().Be(BillingAccountStatus.Restricted);

        account.Reactivate(ActorUserId, now.AddMinutes(1)).IsSuccess.Should().BeTrue();
        account.Status.Should().Be(BillingAccountStatus.Open);
        account.RestrictionReason.Should().BeNull();
    }

    [Fact]
    public void Suspend_FromRestricted_IsAllowed()
    {
        BillingAccount account = CreateAccount();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        account.Restrict("Payment review", ActorUserId, now);
        var result = account.Suspend("Blocking payment incident", ActorUserId, now.AddMinutes(1));

        result.IsSuccess.Should().BeTrue();
        account.Status.Should().Be(BillingAccountStatus.Suspended);
        account.DomainEvents.Should().Contain(e => e is BillingAccountSuspendedDomainEvent);
    }

    [Fact]
    public void RecordInvoiceIssued_UpdatesFinancialTotals()
    {
        BillingAccount account = CreateAccount();

        var result = account.RecordInvoiceIssued(1200m, "EUR", ActorUserId, DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        account.TotalInvoiced.Should().Be(1200m);
        account.OutstandingBalance.Should().Be(1200m);
    }

    [Fact]
    public void RecordInvoiceIssued_WithDifferentCurrency_IsRejected()
    {
        BillingAccount account = CreateAccount();

        var result = account.RecordInvoiceIssued(100m, "USD", ActorUserId, DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BillingAccountErrors.CurrencyMismatch);
        account.TotalInvoiced.Should().Be(0m);
    }

    [Fact]
    public void ClosedAccount_CannotBeReactivated()
    {
        BillingAccount account = CreateAccount();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        account.Close("Training file financially closed", ActorUserId, now).IsSuccess.Should().BeTrue();
        var result = account.Reactivate(ActorUserId, now.AddMinutes(1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BillingAccountErrors.ReactivationNotAllowed);
        account.Status.Should().Be(BillingAccountStatus.Closed);
    }

    private static BillingAccount CreateAccount() => BillingAccount.CreateForStudent(
        BillingAccountId.New(),
        OrganizationId,
        StudentId,
        "EUR").Value;
}
