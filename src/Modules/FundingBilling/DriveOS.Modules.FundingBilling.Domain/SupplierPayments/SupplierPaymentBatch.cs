using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.SupplierPayments;

/// <summary>
/// Groups several supplier payment attempts created in one payment run.
/// It does not own the invoices; each attempt remains independently traceable and reconcilable.
/// </summary>
public sealed class SupplierPaymentBatch:AggregateRoot<SupplierPaymentBatchId>,IAuditableEntity
{
    private SupplierPaymentBatch(){}
    private SupplierPaymentBatch(
        SupplierPaymentBatchId id,OrganizationId organizationId,string paymentMethod,
        string currency,DateOnly scheduledDate,string? reference):base(id)
    {
        OrganizationId=organizationId;
        PaymentMethod=(paymentMethod??string.Empty).Trim();
        Currency=(currency??string.Empty).Trim().ToUpperInvariant();
        ScheduledDate=scheduledDate;
        Reference=string.IsNullOrWhiteSpace(reference)?null:reference.Trim()[..Math.Min(reference.Trim().Length,160)];
        Status=SupplierPaymentBatchStatus.Prepared;
    }

    public OrganizationId OrganizationId{get;private set;}
    public string PaymentMethod{get;private set;}=string.Empty;
    public string Currency{get;private set;}=string.Empty;
    public DateOnly ScheduledDate{get;private set;}
    public string? Reference{get;private set;}
    public SupplierPaymentBatchStatus Status{get;private set;}
    public int ItemCount{get;private set;}
    public decimal TotalAmount{get;private set;}
    public DateTimeOffset? SubmittedAtUtc{get;private set;}

    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<SupplierPaymentBatch> Create(
        SupplierPaymentBatchId id,OrganizationId organizationId,string paymentMethod,string currency,
        DateOnly scheduledDate,string? reference,DateTimeOffset now,UserId actor)
    {
        if(id.IsEmpty||organizationId.IsEmpty||string.IsNullOrWhiteSpace(paymentMethod)||
           string.IsNullOrWhiteSpace(currency)||currency.Trim().Length!=3)
            return Result.Failure<SupplierPaymentBatch>(SupplierPaymentAttemptErrors.InvalidIdentifier);

        var batch=new SupplierPaymentBatch(id,organizationId,paymentMethod,currency,scheduledDate,reference);
        batch.SetCreatedAudit(now,actor);
        return Result.Success(batch);
    }

    public Result SetTotals(int itemCount,decimal totalAmount,DateTimeOffset now,UserId actor)
    {
        if(Status!=SupplierPaymentBatchStatus.Prepared||itemCount<=0||totalAmount<=0)
            return Result.Failure(SupplierPaymentAttemptErrors.InvalidAmount);
        ItemCount=itemCount;
        TotalAmount=decimal.Round(totalAmount,2,MidpointRounding.AwayFromZero);
        Status=SupplierPaymentBatchStatus.Submitted;
        SubmittedAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
}

public enum SupplierPaymentBatchStatus{Prepared=1,Submitted=2,Completed=3,PartiallyFailed=4,Cancelled=5}
