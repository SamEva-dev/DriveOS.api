using DriveOS.Modules.FundingBilling.Domain.SupplierPayments.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.SupplierPayments;

/// <summary>
/// One execution attempt to pay an approved supplier invoice.
/// It models outbound settlement independently from customer Payment/BillingAccount flows.
/// Failed attempts are preserved and a later retry creates a new attempt.
/// </summary>
public sealed class SupplierPaymentAttempt:AggregateRoot<SupplierPaymentAttemptId>,IAuditableEntity
{
    private SupplierPaymentAttempt(){}

    private SupplierPaymentAttempt(
        SupplierPaymentAttemptId id,
        SupplierInvoiceId supplierInvoiceId,
        OrganizationId clientOrganizationId,
        Guid supplierOrganizationId,
        decimal amount,
        string currency,
        string paymentMethod,
        DateOnly scheduledDate,
        string? bankReference,
        SupplierPaymentBatchId? batchId,
        bool manual):base(id)
    {
        SupplierInvoiceId=supplierInvoiceId;
        ClientOrganizationId=clientOrganizationId;
        SupplierOrganizationId=supplierOrganizationId;
        Amount=Money(amount);
        Currency=NormalizeCurrency(currency);
        PaymentMethod=NormalizeToken(paymentMethod);
        ScheduledDate=scheduledDate;
        BankReference=NormalizeOptional(bankReference,160);
        BatchId=batchId;
        IsManual=manual;
        Status=SupplierPaymentAttemptStatus.Scheduled;
    }

    public SupplierInvoiceId SupplierInvoiceId{get;private set;}
    public OrganizationId ClientOrganizationId{get;private set;}
    public Guid SupplierOrganizationId{get;private set;}
    public decimal Amount{get;private set;}
    public string Currency{get;private set;}=string.Empty;
    public string PaymentMethod{get;private set;}=string.Empty;
    public DateOnly ScheduledDate{get;private set;}
    public string? BankReference{get;private set;}
    public SupplierPaymentBatchId? BatchId{get;private set;}
    public bool IsManual{get;private set;}
    public SupplierPaymentAttemptStatus Status{get;private set;}
    public decimal? SettledAmount{get;private set;}
    public DateOnly? SettledOn{get;private set;}
    public decimal? ReconciliationDifference{get;private set;}
    public SupplierPaymentReconciliationStatus ReconciliationStatus{get;private set;}

    public DateTimeOffset? ProcessingAtUtc{get;private set;}
    public DateTimeOffset? PaidAtUtc{get;private set;}
    public DateTimeOffset? FailedAtUtc{get;private set;}
    public DateTimeOffset? CancelledAtUtc{get;private set;}
    public string? ProviderReference{get;private set;}
    public string? FailureReason{get;private set;}

    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<SupplierPaymentAttempt> Schedule(
        SupplierPaymentAttemptId id,
        SupplierInvoiceId supplierInvoiceId,
        OrganizationId clientOrganizationId,
        Guid supplierOrganizationId,
        decimal amount,
        string currency,
        string paymentMethod,
        DateOnly scheduledDate,
        string? bankReference,
        DateTimeOffset now,
        UserId actor,
        SupplierPaymentBatchId? batchId=null,
        bool manual=false)
    {
        if(id.IsEmpty||supplierInvoiceId.IsEmpty||clientOrganizationId.IsEmpty||supplierOrganizationId==Guid.Empty)
            return Result.Failure<SupplierPaymentAttempt>(SupplierPaymentAttemptErrors.InvalidIdentifier);

        if(amount<=0)
            return Result.Failure<SupplierPaymentAttempt>(SupplierPaymentAttemptErrors.InvalidAmount);

        string normalizedCurrency=NormalizeCurrency(currency);
        if(normalizedCurrency.Length!=3||!normalizedCurrency.All(c=>c is>='A' and<='Z'))
            return Result.Failure<SupplierPaymentAttempt>(SupplierPaymentAttemptErrors.InvalidCurrency);

        string method=NormalizeToken(paymentMethod);
        if(method.Length is <2 or >80)
            return Result.Failure<SupplierPaymentAttempt>(SupplierPaymentAttemptErrors.InvalidPaymentMethod);

        var attempt=new SupplierPaymentAttempt(
            id,supplierInvoiceId,clientOrganizationId,supplierOrganizationId,amount,
            normalizedCurrency,method,scheduledDate,bankReference,batchId,manual);

        attempt.SetCreatedAudit(now,actor);
        attempt.RaiseDomainEvent(new SupplierPaymentScheduledDomainEvent(
            attempt.Id,attempt.SupplierInvoiceId,attempt.ClientOrganizationId,
            attempt.Amount,attempt.Currency,attempt.ScheduledDate,now.ToUniversalTime(),actor));

        return Result.Success(attempt);
    }

    public Result MarkProcessing(DateTimeOffset now,UserId actor)
    {
        if(Status!=SupplierPaymentAttemptStatus.Scheduled)
            return Result.Failure(SupplierPaymentAttemptErrors.InvalidTransition);

        Status=SupplierPaymentAttemptStatus.Processing;
        ProcessingAtUtc=now.ToUniversalTime();
        FailureReason=null;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result MarkPaid(
        decimal settledAmount,
        DateOnly settledOn,
        string? providerReference,
        DateTimeOffset now,
        UserId actor)
    {
        if(Status is not SupplierPaymentAttemptStatus.Scheduled and not SupplierPaymentAttemptStatus.Processing)
            return Result.Failure(SupplierPaymentAttemptErrors.InvalidTransition);

        settledAmount=Money(settledAmount);
        if(settledAmount<=0)
            return Result.Failure(SupplierPaymentAttemptErrors.InvalidAmount);

        string? reference=NormalizeOptional(providerReference,250);
        Status=SupplierPaymentAttemptStatus.Paid;
        ProviderReference=reference;
        SettledAmount=settledAmount;
        SettledOn=settledOn;
        ReconciliationDifference=Money(settledAmount-Amount);
        ReconciliationStatus=ReconciliationDifference switch
        {
            0=>SupplierPaymentReconciliationStatus.Exact,
            <0=>SupplierPaymentReconciliationStatus.Underpayment,
            _=>SupplierPaymentReconciliationStatus.Overpayment
        };
        PaidAtUtc=now.ToUniversalTime();
        FailureReason=null;
        SetModifiedAudit(now,actor);

        RaiseDomainEvent(new SupplierPaymentSucceededDomainEvent(
            Id,SupplierInvoiceId,ClientOrganizationId,settledAmount,Currency,reference,PaidAtUtc.Value,actor));

        return Result.Success();
    }

    public Result MarkFailed(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status is not SupplierPaymentAttemptStatus.Scheduled and not SupplierPaymentAttemptStatus.Processing)
            return Result.Failure(SupplierPaymentAttemptErrors.InvalidTransition);

        reason=(reason??"").Trim();
        if(reason.Length is <2 or >1000)
            return Result.Failure(SupplierPaymentAttemptErrors.FailureReasonRequired);

        Status=SupplierPaymentAttemptStatus.Failed;
        FailureReason=reason;
        FailedAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);

        RaiseDomainEvent(new SupplierPaymentFailedDomainEvent(
            Id,SupplierInvoiceId,ClientOrganizationId,Amount,Currency,reason,FailedAtUtc.Value,actor));

        return Result.Success();
    }

    public Result Cancel(DateTimeOffset now,UserId actor)
    {
        if(Status!=SupplierPaymentAttemptStatus.Scheduled)
            return Result.Failure(SupplierPaymentAttemptErrors.InvalidTransition);

        Status=SupplierPaymentAttemptStatus.Cancelled;
        CancelledAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}

    private static decimal Money(decimal value)=>decimal.Round(value,2,MidpointRounding.AwayFromZero);
    private static string NormalizeCurrency(string? value)=>(value??"").Trim().ToUpperInvariant();
    private static string NormalizeToken(string? value)=>(value??"").Trim();
    private static string? NormalizeOptional(string? value,int max)
    {
        value=value?.Trim();
        if(string.IsNullOrEmpty(value))return null;
        return value.Length<=max?value:value[..max];
    }
}

public enum SupplierPaymentAttemptStatus
{
    Scheduled=1,
    Processing=2,
    Paid=3,
    Failed=4,
    Cancelled=5
}

public enum SupplierPaymentReconciliationStatus
{
    Pending=0,
    Exact=1,
    Underpayment=2,
    Overpayment=3
}
