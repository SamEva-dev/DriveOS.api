using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.CreditNotes;

public sealed class CreditNoteLine : Entity<CreditNoteLineId>
{
    private CreditNoteLine() { }
    private CreditNoteLine(CreditNoteLineId id, CreditNoteId creditNoteId, InvoiceLineId? invoiceLineId, string description, decimal quantity, string unit, decimal unitPrice, decimal discountAmount, decimal taxRate) : base(id)
    {
        CreditNoteId = creditNoteId; InvoiceLineId = invoiceLineId; Description = description; Quantity = quantity; Unit = unit; UnitPrice = unitPrice; DiscountAmount = discountAmount; TaxRate = taxRate; Recalculate();
    }
    public CreditNoteId CreditNoteId { get; private set; }
    public InvoiceLineId? InvoiceLineId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    public static Result<CreditNoteLine> Create(CreditNoteLineId id, CreditNoteId creditNoteId, InvoiceLineId? invoiceLineId, string description, decimal quantity, string unit, decimal unitPrice, decimal discountAmount, decimal taxRate)
    {
        string d = description?.Trim() ?? string.Empty; string u = unit?.Trim() ?? string.Empty;
        if (id.IsEmpty || creditNoteId.IsEmpty || d.Length is < 2 or > 500 || u.Length is < 1 or > 50 || quantity <= 0m || unitPrice < 0m || discountAmount < 0m || discountAmount > quantity * unitPrice || taxRate is < 0m or > 100m)
            return Result.Failure<CreditNoteLine>(CreditNoteErrors.InvalidLine);
        return Result.Success(new CreditNoteLine(id, creditNoteId, invoiceLineId, d, decimal.Round(quantity, 4), u, Round(unitPrice), Round(discountAmount), decimal.Round(taxRate, 4)));
    }
    private void Recalculate()
    {
        decimal gross = Round(Quantity * UnitPrice);
        NetAmount = Round(gross - DiscountAmount);
        TaxAmount = Round(NetAmount * TaxRate / 100m);
        TotalAmount = Round(NetAmount + TaxAmount);
    }
    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
