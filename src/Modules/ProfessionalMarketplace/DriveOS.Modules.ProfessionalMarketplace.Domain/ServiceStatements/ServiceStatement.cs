using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceStatements;

/// <summary>
/// Periodic statement grouping external professional service entries.
/// Line decisions remain owned by ServiceEntry; this aggregate snapshots them and derives the statement status.
/// A disputed line never prevents approved lines from continuing toward invoicing.
/// </summary>
public sealed class ServiceStatement:AggregateRoot<ServiceStatementId>,IAuditableEntity
{
    private ServiceStatement(){}
    private ServiceStatement(ServiceStatementId id,ProfessionalEngagementId engagementId,ProfessionalProfileId profileId,
        OrganizationId clientOrganizationId,Guid providerOrganizationId,DateOnly periodStart,DateOnly periodEnd,
        ServiceStatementLine[] lines,string currency):base(id)
    {
        EngagementId=engagementId;ProfessionalProfileId=profileId;ClientOrganizationId=clientOrganizationId;
        ProviderOrganizationId=providerOrganizationId;PeriodStart=periodStart;PeriodEnd=periodEnd;
        Lines=lines;Currency=currency;Status=ServiceStatementStatus.Draft;
    }

    public ProfessionalEngagementId EngagementId{get;private set;}
    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public OrganizationId ClientOrganizationId{get;private set;}
    public Guid ProviderOrganizationId{get;private set;}
    public DateOnly PeriodStart{get;private set;}
    public DateOnly PeriodEnd{get;private set;}
    public ServiceStatementLine[] Lines{get;private set;}=[];
    public string Currency{get;private set;}=string.Empty;
    public decimal TotalAmount=>Lines.Sum(x=>x.TotalAmount);
    public decimal ApprovedAmount=>Lines.Where(x=>x.EntryStatus==ServiceEntryStatus.Approved).Sum(x=>x.TotalAmount);
    public decimal DisputedAmount=>Lines.Where(x=>x.EntryStatus==ServiceEntryStatus.Disputed).Sum(x=>x.TotalAmount);
    public ServiceStatementStatus Status{get;private set;}
    public DateTimeOffset? SubmittedAtUtc{get;private set;}
    public DateTimeOffset? ReviewedAtUtc{get;private set;}
    public UserId? ReviewedByUserId{get;private set;}
    public string? RejectionReason{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<ServiceStatement> Create(ServiceStatementId id,ProfessionalEngagementId engagementId,
        ProfessionalProfileId profileId,OrganizationId clientOrganizationId,Guid providerOrganizationId,
        DateOnly periodStart,DateOnly periodEnd,IEnumerable<ServiceEntry> entries,DateTimeOffset now,UserId actor)
    {
        if(id.IsEmpty||engagementId.IsEmpty||profileId.IsEmpty||clientOrganizationId.IsEmpty||providerOrganizationId==Guid.Empty)
            return Result.Failure<ServiceStatement>(ServiceStatementErrors.InvalidIdentifier);
        if(periodEnd<periodStart)return Result.Failure<ServiceStatement>(ServiceStatementErrors.InvalidPeriod);

        ServiceEntry[] selected=entries
            .Where(x=>x.EngagementId==engagementId&&x.ServiceDate>=periodStart&&x.ServiceDate<=periodEnd)
            .OrderBy(x=>x.ServiceDate).ThenBy(x=>x.CreatedAtUtc).ToArray();

        if(selected.Length==0)return Result.Failure<ServiceStatement>(ServiceStatementErrors.NoEntries);
        if(selected.Any(x=>x.Status==ServiceEntryStatus.Recorded))
            return Result.Failure<ServiceStatement>(ServiceStatementErrors.UnsubmittedEntries);
        string[] currencies=selected.Select(x=>x.Currency).Distinct(StringComparer.Ordinal).ToArray();
        if(currencies.Length!=1)return Result.Failure<ServiceStatement>(ServiceStatementErrors.MixedCurrencies);

        ServiceStatementLine[] lines=selected.Select(Map).ToArray();
        var x=new ServiceStatement(id,engagementId,profileId,clientOrganizationId,providerOrganizationId,periodStart,periodEnd,lines,currencies[0]);
        x.SetCreatedAudit(now,actor);return Result.Success(x);
    }

    public Result Refresh(IEnumerable<ServiceEntry> entries,DateTimeOffset now,UserId actor)
    {
        if(Status==ServiceStatementStatus.Invoiced)return Result.Failure(ServiceStatementErrors.InvalidTransition);
        var byId=entries.ToDictionary(x=>x.Id);
        Lines=Lines.Select(line=>byId.TryGetValue(line.ServiceEntryId,out var entry)?Map(entry):line).ToArray();
        DeriveStatus();
        SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result Submit(DateTimeOffset now,UserId actor)
    {
        if(Status!=ServiceStatementStatus.Draft)return Result.Failure(ServiceStatementErrors.InvalidTransition);
        if(Lines.Any(x=>x.EntryStatus==ServiceEntryStatus.Recorded))
            return Result.Failure(ServiceStatementErrors.UnsubmittedEntries);
        Status=ServiceStatementStatus.Submitted;SubmittedAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result StartReview(DateTimeOffset now,UserId actor)
    {
        if(Status!=ServiceStatementStatus.Submitted)return Result.Failure(ServiceStatementErrors.InvalidTransition);
        Status=ServiceStatementStatus.UnderReview;ReviewedAtUtc=now.ToUniversalTime();ReviewedByUserId=actor;
        SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result RecalculateReviewStatus(DateTimeOffset now,UserId actor)
    {
        if(Status is not ServiceStatementStatus.Submitted and not ServiceStatementStatus.UnderReview and
           not ServiceStatementStatus.PartiallyApproved and not ServiceStatementStatus.Disputed)
            return Result.Failure(ServiceStatementErrors.InvalidTransition);
        DeriveStatus();ReviewedAtUtc=now.ToUniversalTime();ReviewedByUserId=actor;
        SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result Reject(string reason,DateTimeOffset now,UserId actor)
    {
        if((Status is ServiceStatementStatus.Approved or ServiceStatementStatus.Invoiced)||ApprovedAmount>0||DisputedAmount>0)
            return Result.Failure(ServiceStatementErrors.InvalidTransition);
        reason=(reason??"").Trim();
        if(reason.Length is <2 or >512)return Result.Failure(ServiceStatementErrors.ReasonRequired);
        Status=ServiceStatementStatus.Rejected;RejectionReason=reason;ReviewedAtUtc=now.ToUniversalTime();ReviewedByUserId=actor;
        SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result MarkInvoiced(DateTimeOffset now,UserId actor)
    {
        if(Status is not ServiceStatementStatus.Approved and not ServiceStatementStatus.PartiallyApproved)
            return Result.Failure(ServiceStatementErrors.InvoiceNotAllowed);
        if(ApprovedAmount<=0)return Result.Failure(ServiceStatementErrors.InvoiceNotAllowed);
        Status=ServiceStatementStatus.Invoiced;SetModifiedAudit(now,actor);return Result.Success();
    }

    private void DeriveStatus()
    {
        if(Lines.All(x=>x.EntryStatus==ServiceEntryStatus.Approved)){Status=ServiceStatementStatus.Approved;return;}
        if(Lines.Any(x=>x.EntryStatus==ServiceEntryStatus.Disputed))
        {
            Status=Lines.Any(x=>x.EntryStatus==ServiceEntryStatus.Approved)?ServiceStatementStatus.PartiallyApproved:ServiceStatementStatus.Disputed;
            return;
        }
        if(Lines.Any(x=>x.EntryStatus==ServiceEntryStatus.Approved))
        {
            Status=ServiceStatementStatus.PartiallyApproved;return;
        }
        if(Lines.All(x=>x.EntryStatus==ServiceEntryStatus.Rejected))
        {
            Status=ServiceStatementStatus.Rejected;return;
        }
        Status=ServiceStatementStatus.UnderReview;
    }

    private static ServiceStatementLine Map(ServiceEntry x)=>new(
        x.Id,x.ServiceDate,x.ServiceCode,x.QuantityMinutes,x.UnitRate,x.Currency,x.TotalAmount,x.Description,x.Status);

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
}

public sealed record ServiceStatementLine(
    ServiceEntryId ServiceEntryId,DateOnly ServiceDate,string ServiceCode,int QuantityMinutes,
    decimal UnitRate,string Currency,decimal TotalAmount,string Description,ServiceEntryStatus EntryStatus);

public enum ServiceStatementStatus
{
    Draft=1,Submitted=2,UnderReview=3,Approved=4,PartiallyApproved=5,Rejected=6,Disputed=7,Invoiced=8
}
