using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;

/// <summary>
/// Billable service fact performed by an external professional under an engagement/mission.
/// It is evidence-oriented: source session/reference, applied rate snapshot and approval status are preserved.
/// A disputed/rejected line remains independent from other service entries.
/// </summary>
public sealed class ServiceEntry:AggregateRoot<ServiceEntryId>,IAuditableEntity
{
    private ServiceEntry(){}
    private ServiceEntry(ServiceEntryId id,ProfessionalEngagementId engagementId,ProfessionalMissionId? missionId,
        ProfessionalProfileId profileId,OrganizationId organizationId,BranchId? branchId,
        ServiceEntrySourceType sourceType,Guid sourceId,DateOnly serviceDate,string serviceCode,
        int quantityMinutes,decimal unitRate,decimal expensesAmount,decimal indemnitiesAmount,
        decimal discountAmount,string currency,string description):base(id)
    {
        EngagementId=engagementId;MissionId=missionId;ProfessionalProfileId=profileId;OrganizationId=organizationId;
        BranchId=branchId;SourceType=sourceType;SourceId=sourceId;ServiceDate=serviceDate;ServiceCode=Token(serviceCode);
        QuantityMinutes=quantityMinutes;
        UnitRate=Money(unitRate);
        ExpensesAmount=Money(expensesAmount);
        IndemnitiesAmount=Money(indemnitiesAmount);
        DiscountAmount=Money(discountAmount);
        Currency=Token(currency);
        Description=(description??"").Trim();Status=ServiceEntryStatus.Recorded;
    }

    public ProfessionalEngagementId EngagementId{get;private set;}
    public ProfessionalMissionId? MissionId{get;private set;}
    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public OrganizationId OrganizationId{get;private set;}
    public BranchId? BranchId{get;private set;}
    public ServiceEntrySourceType SourceType{get;private set;}
    public Guid SourceId{get;private set;}
    public DateOnly ServiceDate{get;private set;}
    public string ServiceCode{get;private set;}=string.Empty;
    public int QuantityMinutes{get;private set;}
    public decimal UnitRate{get;private set;}
    public decimal ExpensesAmount{get;private set;}
    public decimal IndemnitiesAmount{get;private set;}
    public decimal DiscountAmount{get;private set;}
    public decimal BaseAmount=>Money(UnitRate*QuantityMinutes/60m);
    public string Currency{get;private set;}=string.Empty;
    public decimal TotalAmount=>Money(BaseAmount+ExpensesAmount+IndemnitiesAmount-DiscountAmount);
    public string Description{get;private set;}=string.Empty;
    public ServiceEntryStatus Status{get;private set;}
    public DateTimeOffset? SubmittedAtUtc{get;private set;}
    public DateTimeOffset? ReviewedAtUtc{get;private set;}
    public UserId? ReviewedByUserId{get;private set;}
    public string? ReviewReason{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<ServiceEntry> Create(ServiceEntryId id,ProfessionalEngagementId engagementId,
        ProfessionalMissionId? missionId,ProfessionalProfileId profileId,OrganizationId organizationId,BranchId? branchId,
        ServiceEntrySourceType sourceType,Guid sourceId,DateOnly serviceDate,string serviceCode,int quantityMinutes,
        decimal unitRate,decimal expensesAmount,decimal indemnitiesAmount,decimal discountAmount,
        string currency,string description,DateOnly engagementStartsOn,DateOnly engagementEndsOn,
        DateTimeOffset now,UserId actor)
    {
        if(id.IsEmpty||engagementId.IsEmpty||profileId.IsEmpty||organizationId.IsEmpty||sourceId==Guid.Empty)
            return Result.Failure<ServiceEntry>(ServiceEntryErrors.InvalidIdentifier);
        if(serviceDate<engagementStartsOn||serviceDate>engagementEndsOn)
            return Result.Failure<ServiceEntry>(ServiceEntryErrors.OutsideEngagementPeriod);
        if(quantityMinutes is <=0 or >1440||unitRate<0||expensesAmount<0||indemnitiesAmount<0||discountAmount<0||
           Token(currency).Length!=3||Token(serviceCode).Length is <1 or >80)
            return Result.Failure<ServiceEntry>(ServiceEntryErrors.InvalidService);
        decimal gross=Money(unitRate*quantityMinutes/60m+expensesAmount+indemnitiesAmount);
        if(discountAmount>gross)
            return Result.Failure<ServiceEntry>(ServiceEntryErrors.InvalidService);
        string d=(description??"").Trim();
        if(d.Length is <2 or >1000)return Result.Failure<ServiceEntry>(ServiceEntryErrors.InvalidService);
        var x=new ServiceEntry(id,engagementId,missionId,profileId,organizationId,branchId,sourceType,sourceId,serviceDate,
            serviceCode,quantityMinutes,unitRate,expensesAmount,indemnitiesAmount,discountAmount,currency,d);
        x.SetCreatedAudit(now,actor);return Result.Success(x);
    }

    public Result Submit(DateTimeOffset now,UserId actor)
    {
        if(Status!=ServiceEntryStatus.Recorded)return Result.Failure(ServiceEntryErrors.InvalidTransition);
        Status=ServiceEntryStatus.Submitted;SubmittedAtUtc=now.ToUniversalTime();SetModifiedAudit(now,actor);return Result.Success();
    }
    public Result Approve(DateTimeOffset now,UserId actor)
    {
        if(Status is not ServiceEntryStatus.Submitted and not ServiceEntryStatus.Disputed)
            return Result.Failure(ServiceEntryErrors.InvalidTransition);
        Status=ServiceEntryStatus.Approved;ReviewedAtUtc=now.ToUniversalTime();ReviewedByUserId=actor;ReviewReason=null;
        SetModifiedAudit(now,actor);return Result.Success();
    }
    public Result Reject(string reason,DateTimeOffset now,UserId actor)=>Review(ServiceEntryStatus.Rejected,reason,now,actor);
    public Result OpenDispute(string reason,DateTimeOffset now,UserId actor)=>Review(ServiceEntryStatus.Disputed,reason,now,actor);
    [Obsolete("Use ServiceDispute workflow.")]
    public Result Dispute(string reason,DateTimeOffset now,UserId actor)=>OpenDispute(reason,now,actor);

    public Result ResolveDisputeRejected(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status!=ServiceEntryStatus.Disputed)return Result.Failure(ServiceEntryErrors.InvalidTransition);
        reason=(reason??string.Empty).Trim();
        if(reason.Length is <2 or >512)return Result.Failure(ServiceEntryErrors.ReasonRequired);
        Status=ServiceEntryStatus.Rejected;ReviewReason=reason;ReviewedAtUtc=now.ToUniversalTime();ReviewedByUserId=actor;
        SetModifiedAudit(now,actor);return Result.Success();
    }

    private Result Review(ServiceEntryStatus target,string reason,DateTimeOffset now,UserId actor)
    {
        if(Status!=ServiceEntryStatus.Submitted)return Result.Failure(ServiceEntryErrors.InvalidTransition);
        reason=(reason??"").Trim();
        if(reason.Length is <2 or >512)return Result.Failure(ServiceEntryErrors.ReasonRequired);
        Status=target;ReviewReason=reason;ReviewedAtUtc=now.ToUniversalTime();ReviewedByUserId=actor;
        SetModifiedAudit(now,actor);return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string Token(string? value)=>(value??"").Trim().ToUpperInvariant();
    private static decimal Money(decimal value)=>decimal.Round(value,2,MidpointRounding.AwayFromZero);
}
public enum ServiceEntryStatus{Recorded=1,Submitted=2,Approved=3,Rejected=4,Disputed=5}
public enum ServiceEntrySourceType{TrainingSession=1,MissionActivity=2,ManualAdjustment=3}
