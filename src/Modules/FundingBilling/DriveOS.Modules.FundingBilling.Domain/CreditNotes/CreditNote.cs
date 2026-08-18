using DriveOS.Modules.FundingBilling.Domain.CreditNotes.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.CreditNotes;

public sealed class CreditNote : AggregateRoot<CreditNoteId>, IAuditableEntity
{
    private readonly List<CreditNoteLine> _lines = [];
    private CreditNote() { }
    private CreditNote(CreditNoteId id, OrganizationId organizationId, BillingAccountId billingAccountId, InvoiceId invoiceId, string currency, string reason) : base(id)
    { OrganizationId=organizationId; BillingAccountId=billingAccountId; InvoiceId=invoiceId; Currency=currency; Reason=reason; Status=CreditNoteStatus.Draft; }

    public OrganizationId OrganizationId { get; private set; }
    public BillingAccountId BillingAccountId { get; private set; }
    public InvoiceId InvoiceId { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public string? CreditNoteNumber { get; private set; }
    public DateOnly? IssueDate { get; private set; }
    public CreditNoteStatus Status { get; private set; }
    public IReadOnlyCollection<CreditNoteLine> Lines => _lines.AsReadOnly();
    public decimal Subtotal => Round(_lines.Sum(x=>x.NetAmount));
    public decimal TaxAmount => Round(_lines.Sum(x=>x.TaxAmount));
    public decimal TotalAmount => Round(_lines.Sum(x=>x.TotalAmount));
    public UserId? IssuedByUserId { get; private set; }
    public DateTimeOffset? IssuedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<CreditNote> CreateDraft(CreditNoteId id, OrganizationId organizationId, BillingAccountId billingAccountId, InvoiceId invoiceId, string currency, string reason)
    {
        if(id.IsEmpty) return Result.Failure<CreditNote>(CreditNoteErrors.InvalidIdentifier);
        if(organizationId.IsEmpty || billingAccountId.IsEmpty || invoiceId.IsEmpty) return Result.Failure<CreditNote>(CreditNoteErrors.InvalidOwner);
        string c=currency?.Trim().ToUpperInvariant()??string.Empty; if(c.Length!=3 || !c.All(x=>x is >= 'A' and <= 'Z')) return Result.Failure<CreditNote>(CreditNoteErrors.InvalidCurrency);
        string r=reason?.Trim()??string.Empty; if(r.Length is <3 or >1000) return Result.Failure<CreditNote>(CreditNoteErrors.InvalidReason);
        var note=new CreditNote(id,organizationId,billingAccountId,invoiceId,c,r); note.RaiseDomainEvent(new CreditNoteCreatedDomainEvent(note.Id,note.InvoiceId,note.BillingAccountId,note.Reason)); return Result.Success(note);
    }
    public Result<CreditNoteLineId> AddLine(CreditNoteLineId id, InvoiceLineId? invoiceLineId, string description, decimal quantity, string unit, decimal unitPrice, decimal discountAmount, decimal taxRate)
    {
        if(Status!=CreditNoteStatus.Draft) return Result.Failure<CreditNoteLineId>(CreditNoteErrors.ModificationNotAllowed);
        var result=CreditNoteLine.Create(id,Id,invoiceLineId,description,quantity,unit,unitPrice,discountAmount,taxRate); if(result.IsFailure) return Result.Failure<CreditNoteLineId>(result.Error); _lines.Add(result.Value); return Result.Success(result.Value.Id);
    }
    public Result RemoveLine(CreditNoteLineId id)
    { if(Status!=CreditNoteStatus.Draft) return Result.Failure(CreditNoteErrors.ModificationNotAllowed); var line=_lines.SingleOrDefault(x=>x.Id==id); if(line is null) return Result.Failure(CreditNoteErrors.LineNotFound); _lines.Remove(line); return Result.Success(); }
    public Result Issue(string number, DateOnly issueDate, decimal maximumCreditableAmount, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if(Status!=CreditNoteStatus.Draft) return Result.Failure(CreditNoteErrors.IssueNotAllowed);
        if(_lines.Count==0) return Result.Failure(CreditNoteErrors.Empty);
        string n=number?.Trim()??string.Empty; if(n.Length is <3 or >80) return Result.Failure(CreditNoteErrors.InvalidNumber);
        if(issueDate==default || actorUserId.IsEmpty || occurredAtUtc==default) return Result.Failure(CreditNoteErrors.InvalidActor);
        if(TotalAmount<=0m || TotalAmount>Round(maximumCreditableAmount)) return Result.Failure(CreditNoteErrors.AmountExceeded);
        CreditNoteNumber=n; IssueDate=issueDate; Status=CreditNoteStatus.Issued; IssuedByUserId=actorUserId; IssuedAtUtc=occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new CreditNoteIssuedDomainEvent(Id,InvoiceId,BillingAccountId,CreditNoteNumber,TotalAmount,Currency,actorUserId,IssuedAtUtc.Value)); return Result.Success();
    }
    public void SetCreatedAudit(DateTimeOffset at, UserId? by){if(CreatedAtUtc!=default)return;CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=by;}
    public void SetModifiedAudit(DateTimeOffset at, UserId? by){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=by;}
    private static decimal Round(decimal value)=>decimal.Round(value,2,MidpointRounding.AwayFromZero);
}
