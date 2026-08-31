using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.SupplierPayments;

public sealed class SupplierPaymentRefund:AggregateRoot<SupplierPaymentRefundId>,IAuditableEntity
{
    private SupplierPaymentRefund(){}
    private SupplierPaymentRefund(
        SupplierPaymentRefundId id,SupplierInvoiceId invoiceId,OrganizationId organizationId,
        Guid supplierOrganizationId,decimal amount,string currency,string reason,
        string method,string? providerReference):base(id)
    {
        SupplierInvoiceId=invoiceId;
        ClientOrganizationId=organizationId;
        SupplierOrganizationId=supplierOrganizationId;
        Amount=decimal.Round(amount,2,MidpointRounding.AwayFromZero);
        Currency=(currency??string.Empty).Trim().ToUpperInvariant();
        Reason=reason.Trim();
        Method=(method??string.Empty).Trim();
        ProviderReference=string.IsNullOrWhiteSpace(providerReference)?null:providerReference.Trim();
    }

    public SupplierInvoiceId SupplierInvoiceId{get;private set;}
    public OrganizationId ClientOrganizationId{get;private set;}
    public Guid SupplierOrganizationId{get;private set;}
    public decimal Amount{get;private set;}
    public string Currency{get;private set;}=string.Empty;
    public string Reason{get;private set;}=string.Empty;
    public string Method{get;private set;}=string.Empty;
    public string? ProviderReference{get;private set;}
    public DateTimeOffset RefundedAtUtc{get;private set;}

    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<SupplierPaymentRefund> Record(
        SupplierPaymentRefundId id,SupplierInvoiceId invoiceId,OrganizationId organizationId,
        Guid supplierOrganizationId,decimal amount,string currency,string reason,
        string method,string? providerReference,DateTimeOffset now,UserId actor)
    {
        reason=(reason??string.Empty).Trim();
        method=(method??string.Empty).Trim();
        if(id.IsEmpty||invoiceId.IsEmpty||organizationId.IsEmpty||supplierOrganizationId==Guid.Empty||
           amount<=0||reason.Length is <2 or >512||method.Length is <2 or >80)
            return Result.Failure<SupplierPaymentRefund>(SupplierPaymentAttemptErrors.InvalidAmount);

        var x=new SupplierPaymentRefund(id,invoiceId,organizationId,supplierOrganizationId,
            amount,currency,reason,method,providerReference);
        x.RefundedAtUtc=now.ToUniversalTime();
        x.SetCreatedAudit(now,actor);
        return Result.Success(x);
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
}
