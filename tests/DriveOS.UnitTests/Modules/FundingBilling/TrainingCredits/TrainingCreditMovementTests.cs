using DriveOS.Modules.FundingBilling.Domain.TrainingCredits;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.FundingBilling.TrainingCredits;

public sealed class TrainingCreditMovementTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 18, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void PurchaseReserveConsume_ShouldKeepConsistentBalances()
    {
        TrainingCreditAccount account = CreateAccount();
        UserId actor = new(Guid.NewGuid());

        account.Purchase(TrainingCreditMovementId.New(), 10m, "ORDER-1", null, actor, Now).IsSuccess.Should().BeTrue();
        account.Reserve(TrainingCreditMovementId.New(), 2m, "BOOKING-1", null, actor, Now).IsSuccess.Should().BeTrue();
        account.Consume(TrainingCreditMovementId.New(), 2m, "SESSION-1", null, actor, Now).IsSuccess.Should().BeTrue();

        account.QuantityPurchased.Should().Be(10m);
        account.QuantityReserved.Should().Be(0m);
        account.QuantityConsumed.Should().Be(2m);
        account.QuantityAvailable.Should().Be(8m);
        account.Movements.Should().HaveCount(3);
    }

    [Fact]
    public void Reserve_ShouldRejectQuantityAboveAvailable()
    {
        TrainingCreditAccount account = CreateAccount();
        UserId actor = new(Guid.NewGuid());
        account.Purchase(TrainingCreditMovementId.New(), 1m, "ORDER-1", null, actor, Now);

        var result = account.Reserve(TrainingCreditMovementId.New(), 2m, "BOOKING-1", null, actor, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TrainingCreditAccountErrors.InsufficientAvailable);
    }

    [Fact]
    public void Release_ShouldRestoreAvailability()
    {
        TrainingCreditAccount account = CreateAccount();
        UserId actor = new(Guid.NewGuid());
        account.Purchase(TrainingCreditMovementId.New(), 5m, "ORDER-1", null, actor, Now);
        account.Reserve(TrainingCreditMovementId.New(), 2m, "BOOKING-1", null, actor, Now);

        account.Release(TrainingCreditMovementId.New(), 2m, "BOOKING-CANCELLED-1", "Booking cancelled", actor, Now);

        account.QuantityReserved.Should().Be(0m);
        account.QuantityAvailable.Should().Be(5m);
    }

    [Fact]
    public void Consume_ShouldRequireReservedCredits()
    {
        TrainingCreditAccount account = CreateAccount();
        UserId actor = new(Guid.NewGuid());
        account.Purchase(TrainingCreditMovementId.New(), 5m, "ORDER-1", null, actor, Now);

        var result = account.Consume(TrainingCreditMovementId.New(), 1m, "SESSION-1", null, actor, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TrainingCreditAccountErrors.InsufficientReserved);
    }

    [Fact]
    public void NegativeAdjustment_ShouldNotMakeAvailableNegative()
    {
        TrainingCreditAccount account = CreateAccount();
        UserId actor = new(Guid.NewGuid());
        account.Purchase(TrainingCreditMovementId.New(), 1m, "ORDER-1", null, actor, Now);

        var result = account.Adjust(TrainingCreditMovementId.New(), -2m, "ADJ-1", "Correction", actor, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TrainingCreditAccountErrors.AdjustmentWouldOverdraw);
    }

    private static TrainingCreditAccount CreateAccount() => TrainingCreditAccount.Create(
        TrainingCreditAccountId.New(), new OrganizationId(Guid.NewGuid()), BillingAccountId.New(),
        "PRACTICAL-HOURS", new DateOnly(2027, 12, 31), new DateOnly(2026, 8, 18)).Value;
}
