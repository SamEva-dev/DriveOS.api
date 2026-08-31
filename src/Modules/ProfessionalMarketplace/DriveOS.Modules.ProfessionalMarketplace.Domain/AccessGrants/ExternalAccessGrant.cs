using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;

/// <summary>
/// Resource-scoped, time-bound access granted to an external professional.
/// This is not an AuthGate global permission and never turns the professional into an internal member.
/// </summary>
public sealed class ExternalAccessGrant:AggregateRoot<ExternalAccessGrantId>,IAuditableEntity
{
    private ExternalAccessGrant(){}
    private ExternalAccessGrant(ExternalAccessGrantId id,ProfessionalEngagementId engagementId,ProfessionalProfileId profileId,
        OrganizationId organizationId,BranchId? branchId,ExternalAccessResourceType resourceType,Guid resourceId,
        string permission,DateOnly startDate,DateOnly endDate,UserId grantedBy):base(id)
    {
        EngagementId=engagementId;ProfessionalProfileId=profileId;OrganizationId=organizationId;BranchId=branchId;
        ResourceType=resourceType;ResourceId=resourceId;Permission=Token(permission);StartDate=startDate;EndDate=endDate;
        GrantedByUserId=grantedBy;Status=ExternalAccessGrantStatus.Active;
    }

    public ProfessionalEngagementId EngagementId{get;private set;}
    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public OrganizationId OrganizationId{get;private set;}
    public BranchId? BranchId{get;private set;}
    public ExternalAccessResourceType ResourceType{get;private set;}
    public Guid ResourceId{get;private set;}
    public string Permission{get;private set;}=string.Empty;
    public DateOnly StartDate{get;private set;}
    public DateOnly EndDate{get;private set;}
    public UserId GrantedByUserId{get;private set;}
    public ExternalAccessGrantStatus Status{get;private set;}
    public DateTimeOffset? RevokedAtUtc{get;private set;}
    public UserId? RevokedByUserId{get;private set;}
    public string? RevocationReason{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public bool IsEffectiveOn(DateOnly date)=>Status==ExternalAccessGrantStatus.Active&&StartDate<=date&&date<=EndDate;

    public static Result<ExternalAccessGrant> Create(ExternalAccessGrantId id,ProfessionalEngagementId engagementId,
        ProfessionalProfileId profileId,OrganizationId organizationId,BranchId? branchId,
        ExternalAccessResourceType resourceType,Guid resourceId,string permission,DateOnly startDate,DateOnly endDate,
        DateOnly engagementStartsOn,DateOnly engagementEndsOn,DateTimeOffset now,UserId actor)
    {
        if(id.IsEmpty||engagementId.IsEmpty||profileId.IsEmpty||organizationId.IsEmpty||resourceId==Guid.Empty)
            return Result.Failure<ExternalAccessGrant>(ExternalAccessGrantErrors.InvalidIdentifier);
        string p=Token(permission);
        if(p.Length is <1 or >80||endDate<startDate)
            return Result.Failure<ExternalAccessGrant>(ExternalAccessGrantErrors.InvalidGrant);
        if(startDate<engagementStartsOn||endDate>engagementEndsOn)
            return Result.Failure<ExternalAccessGrant>(ExternalAccessGrantErrors.OutsideEngagementPeriod);
        var x=new ExternalAccessGrant(id,engagementId,profileId,organizationId,branchId,resourceType,resourceId,p,startDate,endDate,actor);
        x.SetCreatedAudit(now,actor);return Result.Success(x);
    }

    public Result Revoke(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status!=ExternalAccessGrantStatus.Active)return Result.Failure(ExternalAccessGrantErrors.InvalidTransition);
        reason=(reason??"").Trim();
        if(reason.Length is <2 or >512)return Result.Failure(ExternalAccessGrantErrors.RevocationReasonRequired);
        Status=ExternalAccessGrantStatus.Revoked;RevokedAtUtc=now.ToUniversalTime();RevokedByUserId=actor;RevocationReason=reason;
        SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result Expire(DateOnly today,DateTimeOffset now)
    {
        if(Status!=ExternalAccessGrantStatus.Active)return Result.Failure(ExternalAccessGrantErrors.InvalidTransition);
        if(today<=EndDate)return Result.Failure(ExternalAccessGrantErrors.NotYetExpired);
        Status=ExternalAccessGrantStatus.Expired;SetModifiedAudit(now,null);return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string Token(string? value)=>(value??"").Trim().ToUpperInvariant();
}
public enum ExternalAccessGrantStatus{Active=1,Revoked=2,Expired=3}
public enum ExternalAccessResourceType
{
    Engagement=1,Mission=2,Student=3,Session=4,Vehicle=5,ContractDocument=6,ServiceStatement=7,Invoice=8,Payment=9
}
