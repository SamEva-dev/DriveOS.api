using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.FundingBilling.Installments;

public sealed class PaymentInstallmentTests
{
    [Fact]
    public void Create_WithValidData_CreatesScheduledInstallment()
    {
        PaymentInstallment installment = CreateInstallment();

        installment.Status.Should().Be(PaymentInstallmentStatus.Scheduled);
        installment.ExpectedAmount.Should().Be(400m);
        installment.PaidAmount.Should().Be(0m);
        installment.RemainingAmount.Should().Be(400m);
        installment.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Create_WithTwoFinancingParties_IsRejected()
    {
        var result = PaymentInstallment.Create(
            PaymentInstallmentId.New(),
            new OrganizationId(Guid.NewGuid()),
            BillingAccountId.New(),
            new DateOnly(2026, 9, 1),
            400m,
            "EUR",
            new PersonId(Guid.NewGuid()),
            new OrganizationId(Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentInstallmentErrors.InvalidFinancingParty);
    }

    [Fact]
    public void Reschedule_ChangesDueDateAndPreservesPreviousDate()
    {
        PaymentInstallment installment = CreateInstallment();
        DateOnly oldDate = installment.DueDate;

        var result = installment.Reschedule(
            new DateOnly(2026, 10, 1),
            "Demande de l'élève",
            new UserId(Guid.NewGuid()),
            DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        installment.PreviousDueDate.Should().Be(oldDate);
        installment.DueDate.Should().Be(new DateOnly(2026, 10, 1));
        installment.Status.Should().Be(PaymentInstallmentStatus.Rescheduled);
    }

    [Fact]
    public void Cancel_PreventsFurtherReschedule()
    {
        PaymentInstallment installment = CreateInstallment();
        installment.Cancel("Annulation du plan", new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow);

        var result = installment.Reschedule(
            new DateOnly(2026, 10, 1),
            "Nouvelle date",
            new UserId(Guid.NewGuid()),
            DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentInstallmentErrors.ModificationNotAllowed);
    }

    [Fact]
    public void Waive_MarksInstallmentAsWaived()
    {
        PaymentInstallment installment = CreateInstallment();

        var result = installment.Waive(
            "Geste commercial exceptionnel",
            new UserId(Guid.NewGuid()),
            DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        installment.Status.Should().Be(PaymentInstallmentStatus.Waived);
        installment.RemainingAmount.Should().Be(0m);
    }

    private static PaymentInstallment CreateInstallment() =>
        PaymentInstallment.Create(
            PaymentInstallmentId.New(),
            new OrganizationId(Guid.NewGuid()),
            BillingAccountId.New(),
            new DateOnly(2026, 9, 1),
            400m,
            "eur").Value;
}
