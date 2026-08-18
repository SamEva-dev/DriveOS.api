using DriveOS.Modules.FundingBilling.Domain.Invoices.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.Invoices;

public sealed class Invoice : AggregateRoot<InvoiceId>, IAuditableEntity
{
    private readonly List<InvoiceLine> _lines = [];

    private Invoice() { }

    private Invoice(
        InvoiceId id,
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        PersonId customerPersonId,
        string currency)
        : base(id)
    {
        OrganizationId = organizationId;
        BillingAccountId = billingAccountId;
        CustomerPersonId = customerPersonId;
        Currency = currency;
        Status = InvoiceStatus.Draft;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BillingAccountId BillingAccountId { get; private set; }
    public PersonId CustomerPersonId { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? InvoiceNumber { get; private set; }
    public DateOnly? IssueDate { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

    public decimal Subtotal => decimal.Round(_lines.Sum(x => x.NetAmount), 2, MidpointRounding.AwayFromZero);
    public decimal TaxAmount => decimal.Round(_lines.Sum(x => x.TaxAmount), 2, MidpointRounding.AwayFromZero);
    public decimal TotalAmount => decimal.Round(_lines.Sum(x => x.TotalAmount), 2, MidpointRounding.AwayFromZero);
    public decimal PaidAmount { get; private set; }
    public decimal CreditedAmount { get; private set; }
    public decimal CreditableAmount => decimal.Max(0m, TotalAmount - CreditedAmount);
    public decimal RemainingAmount => decimal.Max(0m, TotalAmount - PaidAmount - CreditedAmount);

    public DateTimeOffset? IssuedAtUtc { get; private set; }
    public DateTimeOffset? OverdueAtUtc { get; private set; }
    public UserId? IssuedByUserId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Invoice> CreateDraft(
        InvoiceId id,
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        PersonId customerPersonId,
        string currency)
    {
        if (id.IsEmpty)
            return Result.Failure<Invoice>(InvoiceErrors.InvalidIdentifier);

        if (organizationId.IsEmpty || billingAccountId.IsEmpty || customerPersonId.IsEmpty)
            return Result.Failure<Invoice>(InvoiceErrors.InvalidOwner);

        string normalizedCurrency = NormalizeCurrency(currency);
        if (!IsValidCurrency(normalizedCurrency))
            return Result.Failure<Invoice>(InvoiceErrors.InvalidCurrency);

        var invoice = new Invoice(id, organizationId, billingAccountId, customerPersonId, normalizedCurrency);
        invoice.RaiseDomainEvent(new InvoiceCreatedDomainEvent(
            invoice.Id,
            invoice.OrganizationId,
            invoice.BillingAccountId,
            invoice.CustomerPersonId,
            invoice.Currency));

        return Result.Success(invoice);
    }

    public Result<InvoiceLineId> AddLine(
        InvoiceLineId lineId,
        string description,
        decimal quantity,
        string unit,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxRate)
    {
        if (Status != InvoiceStatus.Draft)
            return Result.Failure<InvoiceLineId>(InvoiceErrors.ModificationNotAllowed);

        Result<InvoiceLine> lineResult = InvoiceLine.Create(
            lineId,
            Id,
            description,
            quantity,
            unit,
            unitPrice,
            discountAmount,
            taxRate);

        if (lineResult.IsFailure)
            return Result.Failure<InvoiceLineId>(lineResult.Error);

        _lines.Add(lineResult.Value);
        return Result.Success(lineResult.Value.Id);
    }

    public Result RemoveLine(InvoiceLineId lineId)
    {
        if (Status != InvoiceStatus.Draft)
            return Result.Failure(InvoiceErrors.ModificationNotAllowed);

        InvoiceLine? line = _lines.SingleOrDefault(x => x.Id == lineId);
        if (line is null)
            return Result.Failure(InvoiceErrors.LineNotFound);

        _lines.Remove(line);
        return Result.Success();
    }

    public Result Issue(
        string invoiceNumber,
        DateOnly issueDate,
        DateOnly dueDate,
        UserId actorUserId,
        DateTimeOffset occurredAtUtc)
    {
        if (Status != InvoiceStatus.Draft)
            return Result.Failure(InvoiceErrors.IssueNotAllowed);

        if (_lines.Count == 0)
            return Result.Failure(InvoiceErrors.EmptyInvoice);

        string normalizedNumber = invoiceNumber?.Trim() ?? string.Empty;
        if (normalizedNumber.Length is < 3 or > 80)
            return Result.Failure(InvoiceErrors.InvalidInvoiceNumber);

        if (issueDate == default || dueDate == default || dueDate < issueDate)
            return Result.Failure(InvoiceErrors.InvalidIssuePeriod);

        if (actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(InvoiceErrors.InvalidActor);

        InvoiceNumber = normalizedNumber;
        IssueDate = issueDate;
        DueDate = dueDate;
        Status = InvoiceStatus.Issued;
        IssuedByUserId = actorUserId;
        IssuedAtUtc = occurredAtUtc.ToUniversalTime();

        RaiseDomainEvent(new InvoiceIssuedDomainEvent(
            Id,
            BillingAccountId,
            InvoiceNumber,
            IssueDate.Value,
            DueDate.Value,
            TotalAmount,
            Currency,
            actorUserId,
            IssuedAtUtc.Value));

        return Result.Success();
    }


    public Result MarkOverdue(DateOnly businessDate, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (InvoiceStatus.Issued or InvoiceStatus.PartiallyPaid))
            return Result.Failure(InvoiceErrors.OverdueNotAllowed);
        if (DueDate is null || businessDate <= DueDate.Value || RemainingAmount <= 0m || occurredAtUtc == default)
            return Result.Failure(InvoiceErrors.NotYetOverdue);

        Status = InvoiceStatus.Overdue;
        OverdueAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new InvoiceOverdueDomainEvent(Id, BillingAccountId, InvoiceNumber, DueDate.Value, RemainingAmount, Currency, OverdueAtUtc.Value));
        return Result.Success();
    }

    public Result RecordPaymentAllocation(decimal amount, string currency, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (InvoiceStatus.Issued or InvoiceStatus.PartiallyPaid or InvoiceStatus.Overdue))
            return Result.Failure(InvoiceErrors.PaymentAllocationNotAllowed);

        if (NormalizeCurrency(currency) != Currency)
            return Result.Failure(InvoiceErrors.CurrencyMismatch);

        decimal roundedAmount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        if (roundedAmount <= 0m || roundedAmount > RemainingAmount)
            return Result.Failure(InvoiceErrors.PaymentAllocationAmountExceeded);
        if (actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(InvoiceErrors.InvalidActor);

        bool wasOverdue = Status == InvoiceStatus.Overdue;
        PaidAmount = decimal.Round(PaidAmount + roundedAmount, 2, MidpointRounding.AwayFromZero);
        Status = RemainingAmount == 0m ? InvoiceStatus.Paid : wasOverdue ? InvoiceStatus.Overdue : InvoiceStatus.PartiallyPaid;
        return Result.Success();
    }

    public Result RecordCreditNoteIssued(decimal amount, string currency, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled || CreditableAmount <= 0m)
            return Result.Failure(InvoiceErrors.CreditNoteNotAllowed);
        if (NormalizeCurrency(currency) != Currency)
            return Result.Failure(InvoiceErrors.CurrencyMismatch);
        decimal roundedAmount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        if (roundedAmount <= 0m || roundedAmount > CreditableAmount)
            return Result.Failure(InvoiceErrors.CreditNoteAmountExceeded);
        if (actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(InvoiceErrors.InvalidActor);
        CreditedAmount = decimal.Round(CreditedAmount + roundedAmount, 2, MidpointRounding.AwayFromZero);
        if (RemainingAmount == 0m && PaidAmount == 0m) Status = InvoiceStatus.Credited;
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;

        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }

    private static string NormalizeCurrency(string currency) => currency?.Trim().ToUpperInvariant() ?? string.Empty;

    private static bool IsValidCurrency(string currency) =>
        currency.Length == 3 && currency.All(character => character is >= 'A' and <= 'Z');
}
