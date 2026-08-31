using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;

/// <summary>
/// Accounts-payable invoice received from a supplier.
/// It is intentionally distinct from customer Invoice: matching, operational approval,
/// financial approval and payment scheduling are separate audited stages.
/// </summary>
public sealed class SupplierInvoice:AggregateRoot<SupplierInvoiceId>,IAuditableEntity
{
    private SupplierInvoice(){}

    private SupplierInvoice(
        SupplierInvoiceId id,
        OrganizationId clientOrganizationId,
        Guid supplierOrganizationId,
        SupplierInvoiceSourceType sourceType,
        Guid externalSourceId,
        Guid? serviceStatementId,
        string? supplierReference,
        DateOnly issueDate,
        DateOnly dueDate,
        string currency,
        decimal subtotal,
        decimal taxAmount,
        string invoiceMode):base(id)
    {
        ClientOrganizationId=clientOrganizationId;
        SupplierOrganizationId=supplierOrganizationId;
        SourceType=sourceType;
        ExternalSourceId=externalSourceId;
        ServiceStatementId=serviceStatementId;
        SupplierReference=NormalizeOptional(supplierReference,80);
        IssueDate=issueDate;
        DueDate=dueDate;
        Currency=NormalizeCurrency(currency);
        Subtotal=Money(subtotal);
        TaxAmount=Money(taxAmount);
        InvoiceMode=NormalizeToken(invoiceMode);
        Status=SupplierInvoiceStatus.Received;
        SettlementStatus=SupplierInvoiceSettlementStatus.Pending;
    }

    public OrganizationId ClientOrganizationId{get;private set;}
    public Guid SupplierOrganizationId{get;private set;}
    public SupplierInvoiceSourceType SourceType{get;private set;}
    public Guid ExternalSourceId{get;private set;}
    public Guid? ServiceStatementId{get;private set;}
    public string? SupplierReference{get;private set;}
    public DateOnly IssueDate{get;private set;}
    public DateOnly DueDate{get;private set;}
    public string Currency{get;private set;}=string.Empty;
    public decimal Subtotal{get;private set;}
    public decimal TaxAmount{get;private set;}
    public decimal TotalAmount=>Money(Subtotal+TaxAmount);
    public string InvoiceMode{get;private set;}=string.Empty;
    public SupplierInvoiceStatus Status{get;private set;}
    public SupplierInvoiceSettlementStatus SettlementStatus{get;private set;}
    public decimal PaidAmount{get;private set;}
    public decimal RefundedAmount{get;private set;}
    public decimal NetPaidAmount=>Money(PaidAmount-RefundedAmount);
    public decimal RemainingAmount=>Math.Max(0,Money(TotalAmount-NetPaidAmount));
    public DateTimeOffset? SettlementUpdatedAtUtc{get;private set;}
    public DateTimeOffset? OverdueAtUtc{get;private set;}

    public DateTimeOffset? MatchedAtUtc{get;private set;}
    public UserId? MatchedByUserId{get;private set;}
    public DateTimeOffset? OperationallyApprovedAtUtc{get;private set;}
    public UserId? OperationallyApprovedByUserId{get;private set;}
    public DateTimeOffset? FinanciallyApprovedAtUtc{get;private set;}
    public UserId? FinanciallyApprovedByUserId{get;private set;}
    public DateTimeOffset? ScheduledForPaymentAtUtc{get;private set;}
    public UserId? ScheduledForPaymentByUserId{get;private set;}
    public DateTimeOffset? PaidAtUtc{get;private set;}
    public string? DecisionReason{get;private set;}

    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<SupplierInvoice> Receive(
        SupplierInvoiceId id,
        OrganizationId clientOrganizationId,
        Guid supplierOrganizationId,
        SupplierInvoiceSourceType sourceType,
        Guid externalSourceId,
        Guid? serviceStatementId,
        string? supplierReference,
        DateOnly issueDate,
        DateOnly dueDate,
        string currency,
        decimal subtotal,
        decimal taxAmount,
        string invoiceMode,
        DateTimeOffset now,
        UserId actor)
    {
        if(id.IsEmpty||clientOrganizationId.IsEmpty||supplierOrganizationId==Guid.Empty||externalSourceId==Guid.Empty)
            return Result.Failure<SupplierInvoice>(SupplierInvoiceErrors.InvalidIdentifier);
        if(issueDate==default||dueDate<issueDate||subtotal<0||taxAmount<0)
            return Result.Failure<SupplierInvoice>(SupplierInvoiceErrors.InvalidAmounts);
        string normalizedCurrency=NormalizeCurrency(currency);
        if(normalizedCurrency.Length!=3||!normalizedCurrency.All(c=>c is>='A' and<='Z'))
            return Result.Failure<SupplierInvoice>(SupplierInvoiceErrors.InvalidCurrency);
        if(Money(subtotal+taxAmount)<=0)
            return Result.Failure<SupplierInvoice>(SupplierInvoiceErrors.InvalidAmounts);

        var invoice=new SupplierInvoice(id,clientOrganizationId,supplierOrganizationId,sourceType,externalSourceId,
            serviceStatementId,supplierReference,issueDate,dueDate,normalizedCurrency,subtotal,taxAmount,invoiceMode);
        invoice.SetCreatedAudit(now,actor);
        invoice.RaiseDomainEvent(new SupplierInvoiceReceivedDomainEvent(
            invoice.Id,invoice.ClientOrganizationId,invoice.SupplierOrganizationId,invoice.SourceType,
            invoice.ExternalSourceId,invoice.SupplierReference,invoice.TotalAmount,invoice.Currency,now.ToUniversalTime()));
        return Result.Success(invoice);
    }

    public Result MarkMatched(UserId actor,DateTimeOffset now)
    {
        if(Status is not SupplierInvoiceStatus.Received and not SupplierInvoiceStatus.PendingMatching)
            return Result.Failure(SupplierInvoiceErrors.InvalidTransition);
        Status=SupplierInvoiceStatus.PendingOperationalApproval;
        MatchedAtUtc=now.ToUniversalTime();MatchedByUserId=actor;DecisionReason=null;
        SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result ApproveOperational(UserId actor,DateTimeOffset now)
    {
        if(Status!=SupplierInvoiceStatus.PendingOperationalApproval)
            return Result.Failure(SupplierInvoiceErrors.InvalidTransition);
        Status=SupplierInvoiceStatus.PendingFinancialApproval;
        OperationallyApprovedAtUtc=now.ToUniversalTime();OperationallyApprovedByUserId=actor;DecisionReason=null;
        SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result ApproveFinancial(UserId actor,DateTimeOffset now)
    {
        if(Status!=SupplierInvoiceStatus.PendingFinancialApproval)
            return Result.Failure(SupplierInvoiceErrors.InvalidTransition);
        Status=SupplierInvoiceStatus.Approved;
        FinanciallyApprovedAtUtc=now.ToUniversalTime();FinanciallyApprovedByUserId=actor;DecisionReason=null;
        SetModifiedAudit(now,actor);
        RaiseDomainEvent(new SupplierInvoiceApprovedDomainEvent(Id,ClientOrganizationId,SupplierOrganizationId,
            ExternalSourceId,TotalAmount,Currency,now.ToUniversalTime(),actor));
        return Result.Success();
    }

    public Result SchedulePayment(decimal amount,UserId actor,DateTimeOffset now)
    {
        if(Status is not SupplierInvoiceStatus.Approved and not SupplierInvoiceStatus.ScheduledForPayment)
            return Result.Failure(SupplierInvoiceErrors.PaymentNotAllowed);

        amount=Money(amount);
        if(amount<=0||amount>RemainingAmount)
            return Result.Failure(SupplierInvoiceErrors.InvalidSettlementAmount);

        Status=SupplierInvoiceStatus.ScheduledForPayment;
        SettlementStatus=SupplierInvoiceSettlementStatus.Scheduled;
        ScheduledForPaymentAtUtc=now.ToUniversalTime();
        ScheduledForPaymentByUserId=actor;
        SettlementUpdatedAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result MarkPaymentProcessing(DateTimeOffset now,UserId actor)
    {
        if(Status!=SupplierInvoiceStatus.ScheduledForPayment)
            return Result.Failure(SupplierInvoiceErrors.PaymentNotAllowed);

        SettlementStatus=SupplierInvoiceSettlementStatus.Processing;
        SettlementUpdatedAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result ApplySettledPayment(decimal amount,DateTimeOffset now,UserId actor)
    {
        if(Status is not SupplierInvoiceStatus.ScheduledForPayment and not SupplierInvoiceStatus.Approved)
            return Result.Failure(SupplierInvoiceErrors.PaymentNotAllowed);

        amount=Money(amount);
        if(amount<=0||amount>RemainingAmount)
            return Result.Failure(SupplierInvoiceErrors.InvalidSettlementAmount);

        PaidAmount=Money(PaidAmount+amount);
        SettlementUpdatedAtUtc=now.ToUniversalTime();
        OverdueAtUtc=null;

        if(RemainingAmount<=0)
        {
            Status=SupplierInvoiceStatus.Paid;
            SettlementStatus=SupplierInvoiceSettlementStatus.Paid;
            PaidAtUtc=now.ToUniversalTime();
        }
        else
        {
            Status=SupplierInvoiceStatus.Approved;
            SettlementStatus=SupplierInvoiceSettlementStatus.PartiallyPaid;
            ScheduledForPaymentAtUtc=null;
            ScheduledForPaymentByUserId=null;
        }

        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result MarkPaymentFailed(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status!=SupplierInvoiceStatus.ScheduledForPayment)
            return Result.Failure(SupplierInvoiceErrors.InvalidTransition);

        reason=(reason??string.Empty).Trim();
        Status=SupplierInvoiceStatus.Approved;
        SettlementStatus=SupplierInvoiceSettlementStatus.Failed;
        SettlementUpdatedAtUtc=now.ToUniversalTime();
        ScheduledForPaymentAtUtc=null;
        ScheduledForPaymentByUserId=null;
        DecisionReason=reason.Length==0?null:reason[..Math.Min(reason.Length,512)];
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result MarkPaymentCancelled(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status!=SupplierInvoiceStatus.ScheduledForPayment)
            return Result.Failure(SupplierInvoiceErrors.InvalidTransition);

        reason=(reason??string.Empty).Trim();
        Status=SupplierInvoiceStatus.Approved;
        SettlementStatus=SupplierInvoiceSettlementStatus.Cancelled;
        SettlementUpdatedAtUtc=now.ToUniversalTime();
        ScheduledForPaymentAtUtc=null;
        ScheduledForPaymentByUserId=null;
        DecisionReason=reason.Length==0?null:reason[..Math.Min(reason.Length,512)];
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result MarkOverdue(DateOnly today,DateTimeOffset now)
    {
        if(today<=DueDate||RemainingAmount<=0||
           Status is SupplierInvoiceStatus.Rejected or SupplierInvoiceStatus.Disputed)
            return Result.Failure(SupplierInvoiceErrors.InvalidTransition);

        if(SettlementStatus==SupplierInvoiceSettlementStatus.Paid)
            return Result.Failure(SupplierInvoiceErrors.InvalidTransition);

        SettlementStatus=SupplierInvoiceSettlementStatus.Overdue;
        OverdueAtUtc??=now.ToUniversalTime();
        SettlementUpdatedAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,null);
        return Result.Success();
    }

    public Result RecordRefund(decimal amount,string reason,DateTimeOffset now,UserId actor)
    {
        amount=Money(amount);
        reason=(reason??string.Empty).Trim();

        if(amount<=0||amount>NetPaidAmount)
            return Result.Failure(SupplierInvoiceErrors.InvalidRefundAmount);
        if(reason.Length is <2 or >512)
            return Result.Failure(SupplierInvoiceErrors.ReasonRequired);

        RefundedAmount=Money(RefundedAmount+amount);
        PaidAtUtc=RemainingAmount<=0?PaidAtUtc:null;
        Status=RemainingAmount<=0?SupplierInvoiceStatus.Paid:SupplierInvoiceStatus.Approved;
        SettlementStatus=NetPaidAmount<=0
            ?SupplierInvoiceSettlementStatus.Refunded
            :RemainingAmount<=0
                ?SupplierInvoiceSettlementStatus.Paid
                :SupplierInvoiceSettlementStatus.PartiallyPaid;
        DecisionReason=reason;
        SettlementUpdatedAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result ReopenAfterFailedPayment(string reason,DateTimeOffset now,UserId actor)=>
        MarkPaymentFailed(reason,now,actor);

    public Result Reject(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status is SupplierInvoiceStatus.Paid or SupplierInvoiceStatus.Rejected)
            return Result.Failure(SupplierInvoiceErrors.InvalidTransition);
        return Decide(SupplierInvoiceStatus.Rejected,reason,now,actor);
    }

    public Result Dispute(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status is SupplierInvoiceStatus.Paid or SupplierInvoiceStatus.Rejected)
            return Result.Failure(SupplierInvoiceErrors.InvalidTransition);
        return Decide(SupplierInvoiceStatus.Disputed,reason,now,actor);
    }

    private Result Decide(SupplierInvoiceStatus target,string reason,DateTimeOffset now,UserId actor)
    {
        reason=(reason??"").Trim();
        if(reason.Length is <2 or >512)return Result.Failure(SupplierInvoiceErrors.ReasonRequired);
        Status=target;DecisionReason=reason;SetModifiedAudit(now,actor);return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static decimal Money(decimal value)=>decimal.Round(value,2,MidpointRounding.AwayFromZero);
    private static string NormalizeCurrency(string? value)=>(value??"").Trim().ToUpperInvariant();
    private static string NormalizeToken(string? value)=>(value??"").Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value,int max){value=value?.Trim();if(string.IsNullOrEmpty(value))return null;return value.Length<=max?value:value[..max];}
}

public enum SupplierInvoiceStatus
{
    Received=1,
    PendingMatching=2,
    PendingOperationalApproval=3,
    PendingFinancialApproval=4,
    Approved=5,
    ScheduledForPayment=6,
    Paid=7,
    Rejected=8,
    Disputed=9
}

public enum SupplierInvoiceSettlementStatus
{
    Pending=1,
    Scheduled=2,
    Processing=3,
    Paid=4,
    PartiallyPaid=5,
    Failed=6,
    Overdue=7,
    Cancelled=8,
    Refunded=9
}

public enum SupplierInvoiceSourceType
{
    ProfessionalMarketplace=1,
    PartnerCenter=2,
    VehicleRental=3,
    Garage=4,
    SoftwareVendor=5,
    AdministrativeProvider=6,
    Other=7
}
