using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.Invoices;

public sealed class InvoiceLine : Entity<InvoiceLineId>
{
    private InvoiceLine() { }

    private InvoiceLine(
        InvoiceLineId id,
        InvoiceId invoiceId,
        string description,
        decimal quantity,
        string unit,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxRate)
        : base(id)
    {
        InvoiceId = invoiceId;
        Description = description;
        Quantity = quantity;
        Unit = unit;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
        TaxRate = taxRate;
        Recalculate();
    }

    public InvoiceId InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    internal static Result<InvoiceLine> Create(
        InvoiceLineId id,
        InvoiceId invoiceId,
        string description,
        decimal quantity,
        string unit,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxRate)
    {
        string normalizedDescription = description?.Trim() ?? string.Empty;
        string normalizedUnit = unit?.Trim() ?? string.Empty;
        decimal grossAmount = decimal.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero);

        if (id.IsEmpty || invoiceId.IsEmpty ||
            normalizedDescription.Length is < 2 or > 500 ||
            normalizedUnit.Length is < 1 or > 40 ||
            quantity <= 0m || unitPrice < 0m || discountAmount < 0m ||
            taxRate is < 0m or > 100m || discountAmount > grossAmount)
        {
            return Result.Failure<InvoiceLine>(InvoiceErrors.InvalidLine);
        }

        return Result.Success(new InvoiceLine(
            id,
            invoiceId,
            normalizedDescription,
            quantity,
            normalizedUnit,
            decimal.Round(unitPrice, 2, MidpointRounding.AwayFromZero),
            decimal.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
            taxRate));
    }

    private void Recalculate()
    {
        decimal gross = decimal.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);
        NetAmount = decimal.Round(gross - DiscountAmount, 2, MidpointRounding.AwayFromZero);
        TaxAmount = decimal.Round(NetAmount * TaxRate / 100m, 2, MidpointRounding.AwayFromZero);
        TotalAmount = NetAmount + TaxAmount;
    }
}
