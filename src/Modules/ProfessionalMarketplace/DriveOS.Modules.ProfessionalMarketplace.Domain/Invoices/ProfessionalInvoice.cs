using DriveOS.Modules.ProfessionalMarketplace.Domain.Events;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceStatements;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Invoices;

/// <summary>
/// Invoice proposal produced from the approved portion of a professional service statement.
/// BC-13 owns preparation and supplier-side validation; BC-07 Finance remains owner of the payable
/// supplier invoice and payment lifecycle after ProfessionalInvoiceRequested is emitted.
/// </summary>
public sealed class ProfessionalInvoice:AggregateRoot<ProfessionalInvoiceId>,IAuditableEntity
{
    private ProfessionalInvoice(){}
    private ProfessionalInvoice(ProfessionalInvoiceId id,ProfessionalEngagementId engagementId,
        ProfessionalProfileId profileId,ServiceStatementId statementId,Guid providerOrganizationId,
        OrganizationId clientOrganizationId,ProfessionalInvoiceMode mode,DateOnly issueDate,DateOnly dueDate,
        string currency,decimal subtotal,decimal taxAmount,string? invoiceNumber,string? bankReference):base(id)
    {
        EngagementId=engagementId;ProfessionalProfileId=profileId;ServiceStatementId=statementId;
        ProviderOrganizationId=providerOrganizationId;ClientOrganizationId=clientOrganizationId;Mode=mode;
        IssueDate=issueDate;DueDate=dueDate;Currency=Token(currency);Subtotal=Money(subtotal);TaxAmount=Money(taxAmount);
        InvoiceNumber=Clean(invoiceNumber,80);BankReference=Clean(bankReference,160);Status=ProfessionalInvoiceStatus.Draft;
        PaymentStatus=ProfessionalInvoicePaymentStatus.Pending;
    }

    public ProfessionalEngagementId EngagementId{get;private set;}
    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public ServiceStatementId ServiceStatementId{get;private set;}
    public Guid ProviderOrganizationId{get;private set;}
    public OrganizationId ClientOrganizationId{get;private set;}
    public ProfessionalInvoiceMode Mode{get;private set;}
    public string? InvoiceNumber{get;private set;}
    public DateOnly IssueDate{get;private set;}
    public DateOnly DueDate{get;private set;}
    public string Currency{get;private set;}=string.Empty;
    public decimal Subtotal{get;private set;}
    public decimal TaxAmount{get;private set;}
    public decimal Total=>Money(Subtotal+TaxAmount);
    public string? BankReference{get;private set;}
    public ProfessionalInvoiceStatus Status{get;private set;}
    public ProfessionalInvoicePaymentStatus PaymentStatus{get;private set;}
    public Guid? FinanceSupplierInvoiceId{get;private set;}
    public string? FinanceSupplierInvoiceStatus{get;private set;}
    public DateTimeOffset? FinanceStatusSyncedAtUtc{get;private set;}
    public DateTimeOffset? ValidatedAtUtc{get;private set;}
    public UserId? ValidatedByUserId{get;private set;}
    public DateTimeOffset? RequestedAtUtc{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<ProfessionalInvoice> Create(ProfessionalInvoiceId id,ServiceStatement statement,
        ProfessionalInvoiceMode mode,DateOnly issueDate,DateOnly dueDate,decimal taxAmount,
        string? invoiceNumber,string? bankReference,DateTimeOffset now,UserId actor)
    {
        if(id.IsEmpty||statement.Id.IsEmpty||statement.EngagementId.IsEmpty||statement.ProfessionalProfileId.IsEmpty||
           statement.ClientOrganizationId.IsEmpty||statement.ProviderOrganizationId==Guid.Empty)
            return Result.Failure<ProfessionalInvoice>(ProfessionalInvoiceErrors.InvalidIdentifier);
        if(statement.Status is not ServiceStatementStatus.Approved and not ServiceStatementStatus.PartiallyApproved)
            return Result.Failure<ProfessionalInvoice>(ProfessionalInvoiceErrors.ApprovedStatementRequired);
        if(statement.ApprovedAmount<=0)return Result.Failure<ProfessionalInvoice>(ProfessionalInvoiceErrors.NoApprovedAmount);
        if(dueDate<issueDate||taxAmount<0)return Result.Failure<ProfessionalInvoice>(ProfessionalInvoiceErrors.InvalidAmounts);
        if(statement.Currency.Length!=3)return Result.Failure<ProfessionalInvoice>(ProfessionalInvoiceErrors.InvalidCurrency);

        string? number=Clean(invoiceNumber,80);
        if(mode==ProfessionalInvoiceMode.FreelanceIssued&&string.IsNullOrWhiteSpace(number))
            return Result.Failure<ProfessionalInvoice>(ProfessionalInvoiceErrors.InvoiceNumberRequired);

        var x=new ProfessionalInvoice(id,statement.EngagementId,statement.ProfessionalProfileId,statement.Id,
            statement.ProviderOrganizationId,statement.ClientOrganizationId,mode,issueDate,dueDate,
            statement.Currency,statement.ApprovedAmount,taxAmount,number,bankReference);
        x.SetCreatedAudit(now,actor);return Result.Success(x);
    }

    public Result UpdateDraft(DateOnly issueDate,DateOnly dueDate,decimal taxAmount,string? invoiceNumber,
        string? bankReference,DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalInvoiceStatus.Draft)return Result.Failure(ProfessionalInvoiceErrors.ImmutableAfterValidation);
        if(dueDate<issueDate||taxAmount<0)return Result.Failure(ProfessionalInvoiceErrors.InvalidAmounts);
        string? number=Clean(invoiceNumber,80);
        if(Mode==ProfessionalInvoiceMode.FreelanceIssued&&string.IsNullOrWhiteSpace(number))
            return Result.Failure(ProfessionalInvoiceErrors.InvoiceNumberRequired);
        IssueDate=issueDate;DueDate=dueDate;TaxAmount=Money(taxAmount);InvoiceNumber=number;BankReference=Clean(bankReference,160);
        SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result Validate(DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalInvoiceStatus.Draft)return Result.Failure(ProfessionalInvoiceErrors.InvalidTransition);
        if(Mode==ProfessionalInvoiceMode.FreelanceIssued&&string.IsNullOrWhiteSpace(InvoiceNumber))
            return Result.Failure(ProfessionalInvoiceErrors.InvoiceNumberRequired);
        Status=ProfessionalInvoiceStatus.Validated;ValidatedAtUtc=now.ToUniversalTime();ValidatedByUserId=actor;
        SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result RequestFinance(Guid supplierInvoiceId,string supplierInvoiceStatus,DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalInvoiceStatus.Validated)return Result.Failure(ProfessionalInvoiceErrors.InvalidTransition);
        if(supplierInvoiceId==Guid.Empty||string.IsNullOrWhiteSpace(supplierInvoiceStatus))
            return Result.Failure(ProfessionalInvoiceErrors.InvalidFinanceReference);

        FinanceSupplierInvoiceId=supplierInvoiceId;
        SyncFinanceStatus(supplierInvoiceStatus,null,now,actor);

        Status=ProfessionalInvoiceStatus.Requested;
        RequestedAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);

        RaiseDomainEvent(new ProfessionalInvoiceRequestedDomainEvent(Guid.NewGuid(),now.ToUniversalTime(),Id,
            ServiceStatementId,ProviderOrganizationId,ClientOrganizationId,InvoiceNumber,IssueDate,DueDate,Currency,
            Subtotal,TaxAmount,Total,Mode.ToString(),actor));
        return Result.Success();
    }

    public Result SyncFinanceStatus(string status,string? latestPaymentStatus,DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalInvoiceStatus.Requested&&FinanceSupplierInvoiceId is null)
            return Result.Failure(ProfessionalInvoiceErrors.InvalidTransition);

        string normalized=(status??string.Empty).Trim();
        if(normalized.Length is <2 or >40)
            return Result.Failure(ProfessionalInvoiceErrors.InvalidFinanceReference);

        FinanceSupplierInvoiceStatus=normalized;
        FinanceStatusSyncedAtUtc=now.ToUniversalTime();

        string payment=(latestPaymentStatus??string.Empty).Trim();
        PaymentStatus=payment switch
        {
            "Scheduled"=>ProfessionalInvoicePaymentStatus.Scheduled,
            "Processing"=>ProfessionalInvoicePaymentStatus.Processing,
            "Paid"=>ProfessionalInvoicePaymentStatus.Paid,
            "Failed"=>ProfessionalInvoicePaymentStatus.Failed,
            "Cancelled"=>ProfessionalInvoicePaymentStatus.Cancelled,
            _=>normalized switch
            {
                "ScheduledForPayment"=>ProfessionalInvoicePaymentStatus.Scheduled,
                "Paid"=>ProfessionalInvoicePaymentStatus.Paid,
                "Rejected"=>ProfessionalInvoicePaymentStatus.Failed,
                "Disputed"=>ProfessionalInvoicePaymentStatus.Failed,
                _=>ProfessionalInvoicePaymentStatus.Pending
            }
        };

        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result Cancel(DateTimeOffset now,UserId actor)
    {
        if(Status==ProfessionalInvoiceStatus.Requested)return Result.Failure(ProfessionalInvoiceErrors.ImmutableAfterRequest);
        Status=ProfessionalInvoiceStatus.Cancelled;SetModifiedAudit(now,actor);return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static decimal Money(decimal x)=>decimal.Round(x,2,MidpointRounding.AwayFromZero);
    private static string Token(string? value)=>(value??"").Trim().ToUpperInvariant();
    private static string? Clean(string? value,int max){value=value?.Trim();if(string.IsNullOrEmpty(value))return null;return value.Length<=max?value:value[..max];}
}

public enum ProfessionalInvoiceMode{FreelanceIssued=1,SelfBilling=2}
public enum ProfessionalInvoiceStatus{Draft=1,Validated=2,Requested=3,Cancelled=4}
public enum ProfessionalInvoicePaymentStatus{Pending=1,Scheduled=2,Processing=3,Paid=4,PartiallyPaid=5,Failed=6,Overdue=7,Cancelled=8,Refunded=9}
