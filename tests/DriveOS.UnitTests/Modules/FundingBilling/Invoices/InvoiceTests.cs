using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.FundingBilling.Invoices;

public sealed class InvoiceTests
{
    [Fact]
    public void CreateDraft_WithValidData_CreatesEmptyDraft()
    {
        Invoice invoice = CreateInvoice();

        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.Lines.Should().BeEmpty();
        invoice.TotalAmount.Should().Be(0m);
        invoice.Currency.Should().Be("EUR");
    }

    [Fact]
    public void AddLine_ComputesNetTaxAndTotal()
    {
        Invoice invoice = CreateInvoice();

        var result = invoice.AddLine(
            InvoiceLineId.New(),
            "Heure de conduite",
            2m,
            "hour",
            50m,
            10m,
            20m);

        result.IsSuccess.Should().BeTrue();
        invoice.Subtotal.Should().Be(90m);
        invoice.TaxAmount.Should().Be(18m);
        invoice.TotalAmount.Should().Be(108m);
    }

    [Fact]
    public void Issue_WithoutLine_IsRejected()
    {
        Invoice invoice = CreateInvoice();

        var result = invoice.Issue(
            "INV-2026-001",
            new DateOnly(2026, 8, 18),
            new DateOnly(2026, 9, 18),
            new UserId(Guid.NewGuid()),
            DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvoiceErrors.EmptyInvoice);
    }

    [Fact]
    public void IssuedInvoice_CannotBeModified()
    {
        Invoice invoice = CreateInvoice();
        invoice.AddLine(InvoiceLineId.New(), "Formation", 1m, "package", 1200m, 0m, 0m);
        invoice.Issue(
            "INV-2026-001",
            new DateOnly(2026, 8, 18),
            new DateOnly(2026, 9, 18),
            new UserId(Guid.NewGuid()),
            DateTimeOffset.UtcNow);

        var addResult = invoice.AddLine(InvoiceLineId.New(), "Extra", 1m, "unit", 50m, 0m, 0m);
        var removeResult = invoice.RemoveLine(invoice.Lines.Single().Id);

        addResult.IsFailure.Should().BeTrue();
        addResult.Error.Should().Be(InvoiceErrors.ModificationNotAllowed);
        removeResult.IsFailure.Should().BeTrue();
        removeResult.Error.Should().Be(InvoiceErrors.ModificationNotAllowed);
    }

    [Fact]
    public void Issue_WithDueDateBeforeIssueDate_IsRejected()
    {
        Invoice invoice = CreateInvoice();
        invoice.AddLine(InvoiceLineId.New(), "Formation", 1m, "package", 1200m, 0m, 0m);

        var result = invoice.Issue(
            "INV-2026-001",
            new DateOnly(2026, 8, 18),
            new DateOnly(2026, 8, 17),
            new UserId(Guid.NewGuid()),
            DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvoiceErrors.InvalidIssuePeriod);
    }

    private static Invoice CreateInvoice() =>
        Invoice.CreateDraft(
            InvoiceId.New(),
            new OrganizationId(Guid.NewGuid()),
            BillingAccountId.New(),
            new PersonId(Guid.NewGuid()),
            "eur").Value;
}
