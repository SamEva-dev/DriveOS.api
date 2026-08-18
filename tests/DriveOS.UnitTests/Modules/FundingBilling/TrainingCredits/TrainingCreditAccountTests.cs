using DriveOS.Modules.FundingBilling.Domain.TrainingCredits;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.FundingBilling.TrainingCredits;

public sealed class TrainingCreditAccountTests
{
    [Fact]
    public void Create_ShouldInitializeEmptyActiveAccount()
    {
        var result = TrainingCreditAccount.Create(
            TrainingCreditAccountId.New(),
            new OrganizationId(Guid.NewGuid()),
            BillingAccountId.New(),
            "practical-hours",
            new DateOnly(2027, 12, 31),
            new DateOnly(2026, 8, 18));

        result.IsSuccess.Should().BeTrue();
        result.Value.CreditType.Should().Be("PRACTICAL-HOURS");
        result.Value.Status.Should().Be(TrainingCreditAccountStatus.Active);
        result.Value.QuantityPurchased.Should().Be(0m);
        result.Value.QuantityReserved.Should().Be(0m);
        result.Value.QuantityConsumed.Should().Be(0m);
        result.Value.Adjustments.Should().Be(0m);
        result.Value.QuantityAvailable.Should().Be(0m);
    }

    [Fact]
    public void Create_ShouldRejectAlreadyExpiredCreditAccount()
    {
        var result = TrainingCreditAccount.Create(
            TrainingCreditAccountId.New(),
            new OrganizationId(Guid.NewGuid()),
            BillingAccountId.New(),
            "THEORY_ACCESS",
            new DateOnly(2026, 8, 17),
            new DateOnly(2026, 8, 18));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TrainingCreditAccountErrors.InvalidExpirationDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("bad type!")]
    public void Create_ShouldRejectInvalidCreditType(string creditType)
    {
        var result = TrainingCreditAccount.Create(
            TrainingCreditAccountId.New(),
            new OrganizationId(Guid.NewGuid()),
            BillingAccountId.New(),
            creditType,
            null,
            new DateOnly(2026, 8, 18));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TrainingCreditAccountErrors.InvalidCreditType);
    }
}
