using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.FundingBilling;

public sealed class PaymentReminderTests
{
    [Fact]
    public void MarkSent_ShouldPersistEmailQueueReference()
    {
        var created = PaymentReminder.Request(
            PaymentReminderId.New(),
            OrganizationId.New(),
            BillingAccountId.New(),
            PaymentReminderTargetType.Invoice,
            Guid.NewGuid(),
            new DateOnly(2026, 8, 1),
            120m,
            "EUR",
            1,
            DateTimeOffset.UtcNow);

        created.IsSuccess.Should().BeTrue();
        Guid emailId = Guid.NewGuid();
        DateTimeOffset sentAt = DateTimeOffset.UtcNow;

        var result = created.Value.MarkSent(emailId, sentAt);

        result.IsSuccess.Should().BeTrue();
        created.Value.Status.Should().Be(PaymentReminderStatus.Sent);
        created.Value.EmailMessageId.Should().Be(emailId);
        created.Value.SentAtUtc.Should().Be(sentAt.ToUniversalTime());
    }

    [Fact]
    public void MarkSent_ShouldRejectSecondTransition()
    {
        var reminder = PaymentReminder.Request(
            PaymentReminderId.New(),
            OrganizationId.New(),
            BillingAccountId.New(),
            PaymentReminderTargetType.Installment,
            Guid.NewGuid(),
            new DateOnly(2026, 8, 1),
            80m,
            "EUR",
            1,
            DateTimeOffset.UtcNow).Value;

        reminder.MarkSent(Guid.NewGuid(), DateTimeOffset.UtcNow);
        var second = reminder.MarkSent(Guid.NewGuid(), DateTimeOffset.UtcNow);

        second.IsFailure.Should().BeTrue();
        second.Error.Should().Be(PaymentReminderErrors.InvalidStatus);
    }
}
