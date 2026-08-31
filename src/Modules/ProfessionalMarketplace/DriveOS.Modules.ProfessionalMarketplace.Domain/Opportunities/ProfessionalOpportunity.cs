using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;

/// <summary>
/// Tenant-owned marketplace need published by an organization.
/// It captures the commercial/search requirements of a future collaboration but does not create
/// a workforce relationship, scheduling booking, contract or financial commitment.
/// </summary>
public sealed class ProfessionalOpportunity : AggregateRoot<ProfessionalOpportunityId>, IAuditableEntity
{
    private ProfessionalOpportunity() { }

    private ProfessionalOpportunity(
        ProfessionalOpportunityId id,
        OrganizationId organizationId,
        BranchId? branchId,
        string title,
        string description,
        ProfessionalType professionalType,
        string[] teachingCategoryCodes,
        string[] requiredLanguageCodes,
        string[] requiredSpecializationCodes,
        string countryCode,
        string? areaCode,
        string? areaDisplayName,
        decimal? latitude,
        decimal? longitude,
        int? radiusKm,
        DateOnly startsOn,
        DateOnly endsOn,
        OpportunityTimeWindow[] timeWindows,
        int? estimatedMinutes,
        ProfessionalEngagementType engagementType,
        ProfessionalVehicleProvisionMode vehicleProvisionMode,
        decimal? budgetMin,
        decimal? budgetMax,
        string? currency,
        ProfessionalRateUnit? budgetUnit,
        bool budgetNegotiable) : base(id)
    {
        OrganizationId=organizationId; BranchId=branchId; Title=title.Trim(); Description=description.Trim();
        ProfessionalType=professionalType; TeachingCategoryCodes=Normalize(teachingCategoryCodes);
        RequiredLanguageCodes=Normalize(requiredLanguageCodes); RequiredSpecializationCodes=Normalize(requiredSpecializationCodes);
        CountryCode=Token(countryCode); AreaCode=OptionalToken(areaCode); AreaDisplayName=Optional(areaDisplayName,160);
        Latitude=NormalizeLatitude(latitude); Longitude=NormalizeLongitude(longitude); RadiusKm=radiusKm;
        StartsOn=startsOn; EndsOn=endsOn; TimeWindows=timeWindows; EstimatedMinutes=estimatedMinutes;
        EngagementType=engagementType; VehicleProvisionMode=vehicleProvisionMode;
        BudgetMin=budgetMin; BudgetMax=budgetMax; Currency=OptionalToken(currency); BudgetUnit=budgetUnit; BudgetNegotiable=budgetNegotiable;
        Status=ProfessionalOpportunityStatus.Draft;
    }

    public OrganizationId OrganizationId{get;private set;}
    public BranchId? BranchId{get;private set;}
    public string Title{get;private set;}=string.Empty;
    public string Description{get;private set;}=string.Empty;
    public ProfessionalType ProfessionalType{get;private set;}
    public string[] TeachingCategoryCodes{get;private set;}=[];
    public string[] RequiredLanguageCodes{get;private set;}=[];
    public string[] RequiredSpecializationCodes{get;private set;}=[];
    public string CountryCode{get;private set;}=string.Empty;
    public string? AreaCode{get;private set;}
    public string? AreaDisplayName{get;private set;}
    public decimal? Latitude{get;private set;}
    public decimal? Longitude{get;private set;}
    public int? RadiusKm{get;private set;}
    public DateOnly StartsOn{get;private set;}
    public DateOnly EndsOn{get;private set;}
    public OpportunityTimeWindow[] TimeWindows{get;private set;}=[];
    public int? EstimatedMinutes{get;private set;}
    public ProfessionalEngagementType EngagementType{get;private set;}
    public ProfessionalVehicleProvisionMode VehicleProvisionMode{get;private set;}
    public decimal? BudgetMin{get;private set;}
    public decimal? BudgetMax{get;private set;}
    public string? Currency{get;private set;}
    public ProfessionalRateUnit? BudgetUnit{get;private set;}
    public bool BudgetNegotiable{get;private set;}
    public ProfessionalOpportunityStatus Status{get;private set;}
    public DateTimeOffset? PublishedAtUtc{get;private set;}
    public DateTimeOffset? ClosedAtUtc{get;private set;}
    public string? ClosureReason{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<ProfessionalOpportunity> Create(
        ProfessionalOpportunityId id,OrganizationId organizationId,BranchId? branchId,string title,string description,
        ProfessionalType professionalType,IEnumerable<string> teachingCategoryCodes,IEnumerable<string>? languageCodes,
        IEnumerable<string>? specializationCodes,string countryCode,string? areaCode,string? areaDisplayName,
        decimal? latitude,decimal? longitude,int? radiusKm,DateOnly startsOn,DateOnly endsOn,
        IEnumerable<OpportunityTimeWindow>? timeWindows,int? estimatedMinutes,ProfessionalEngagementType engagementType,
        ProfessionalVehicleProvisionMode vehicleProvisionMode,decimal? budgetMin,decimal? budgetMax,string? currency,
        ProfessionalRateUnit? budgetUnit,bool budgetNegotiable,DateTimeOffset now,UserId actor)
    {
        var cats=Normalize(teachingCategoryCodes); var langs=Normalize(languageCodes??[]); var specs=Normalize(specializationCodes??[]);
        var windows=(timeWindows??[]).OrderBy(x=>x.DayOfWeek).ThenBy(x=>x.StartTime).ToArray();
        if(id.IsEmpty||organizationId.IsEmpty)return Result.Failure<ProfessionalOpportunity>(ProfessionalOpportunityErrors.InvalidIdentifier);
        if(string.IsNullOrWhiteSpace(title)||title.Trim().Length>180||string.IsNullOrWhiteSpace(description)||description.Trim().Length>4000)
            return Result.Failure<ProfessionalOpportunity>(ProfessionalOpportunityErrors.InvalidContent);
        if(cats.Length==0||countryCode?.Trim().Length!=2||endsOn<startsOn)
            return Result.Failure<ProfessionalOpportunity>(ProfessionalOpportunityErrors.InvalidRequirements);
        if(radiusKm is <0 or >500||(latitude is null)!=(longitude is null))
            return Result.Failure<ProfessionalOpportunity>(ProfessionalOpportunityErrors.InvalidLocation);
        if(windows.Any(x=>x.StartTime>=x.EndTime||string.IsNullOrWhiteSpace(x.TimeZoneId)))
            return Result.Failure<ProfessionalOpportunity>(ProfessionalOpportunityErrors.InvalidTimeWindows);
        foreach(var g in windows.GroupBy(x=>x.DayOfWeek))
        {
            var a=g.OrderBy(x=>x.StartTime).ToArray();
            for(int i=1;i<a.Length;i++)if(a[i].StartTime<a[i-1].EndTime)return Result.Failure<ProfessionalOpportunity>(ProfessionalOpportunityErrors.InvalidTimeWindows);
        }
        if(estimatedMinutes is <=0 or >100000)
            return Result.Failure<ProfessionalOpportunity>(ProfessionalOpportunityErrors.InvalidRequirements);
        if((budgetMin is not null||budgetMax is not null) && (string.IsNullOrWhiteSpace(currency)||currency.Trim().Length!=3||budgetUnit is null))
            return Result.Failure<ProfessionalOpportunity>(ProfessionalOpportunityErrors.InvalidBudget);
        if(budgetMin<0||budgetMax<0||(budgetMin is not null&&budgetMax is not null&&budgetMin>budgetMax))
            return Result.Failure<ProfessionalOpportunity>(ProfessionalOpportunityErrors.InvalidBudget);

        var x=new ProfessionalOpportunity(id,organizationId,branchId,title,description,professionalType,cats,langs,specs,countryCode,areaCode,areaDisplayName,latitude,longitude,radiusKm,startsOn,endsOn,windows,estimatedMinutes,engagementType,vehicleProvisionMode,budgetMin,budgetMax,currency,budgetUnit,budgetNegotiable);
        x.SetCreatedAudit(now,actor); return Result.Success(x);
    }

    public Result Publish(DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalOpportunityStatus.Draft&&Status!=ProfessionalOpportunityStatus.Paused)
            return Result.Failure(ProfessionalOpportunityErrors.InvalidTransition);
        if(EndsOn<DateOnly.FromDateTime(now.UtcDateTime))
            return Result.Failure(ProfessionalOpportunityErrors.OpportunityExpired);
        Status=ProfessionalOpportunityStatus.Published; PublishedAtUtc??=now.ToUniversalTime(); ClosureReason=null; ClosedAtUtc=null; SetModifiedAudit(now,actor); return Result.Success();
    }

    public Result Pause(DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalOpportunityStatus.Published)return Result.Failure(ProfessionalOpportunityErrors.InvalidTransition);
        Status=ProfessionalOpportunityStatus.Paused;SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result Fill(DateTimeOffset now,UserId actor)
    {
        if(Status is not ProfessionalOpportunityStatus.Published and not ProfessionalOpportunityStatus.Paused)
            return Result.Failure(ProfessionalOpportunityErrors.InvalidTransition);
        Status=ProfessionalOpportunityStatus.Filled;ClosedAtUtc=now.ToUniversalTime();SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result Cancel(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status is ProfessionalOpportunityStatus.Filled or ProfessionalOpportunityStatus.Cancelled or ProfessionalOpportunityStatus.Expired)
            return Result.Failure(ProfessionalOpportunityErrors.InvalidTransition);
        reason=(reason??"").Trim();if(reason.Length is <2 or >512)return Result.Failure(ProfessionalOpportunityErrors.ClosureReasonRequired);
        Status=ProfessionalOpportunityStatus.Cancelled;ClosureReason=reason;ClosedAtUtc=now.ToUniversalTime();SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result Expire(DateTimeOffset now)
    {
        if(Status is ProfessionalOpportunityStatus.Filled or ProfessionalOpportunityStatus.Cancelled or ProfessionalOpportunityStatus.Expired)
            return Result.Failure(ProfessionalOpportunityErrors.InvalidTransition);
        if(EndsOn>=DateOnly.FromDateTime(now.UtcDateTime))return Result.Failure(ProfessionalOpportunityErrors.NotYetExpired);
        Status=ProfessionalOpportunityStatus.Expired;ClosedAtUtc=now.ToUniversalTime();SetModifiedAudit(now,null);return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string Token(string? v)=>(v??"").Trim().ToUpperInvariant();
    private static string? OptionalToken(string? v)=>string.IsNullOrWhiteSpace(v)?null:Token(v);
    private static string? Optional(string? v,int max)=>string.IsNullOrWhiteSpace(v)?null:v.Trim()[..Math.Min(v.Trim().Length,max)];
    private static string[] Normalize(IEnumerable<string> v)=>v.Where(x=>!string.IsNullOrWhiteSpace(x)).Select(Token).Distinct(StringComparer.Ordinal).OrderBy(x=>x,StringComparer.Ordinal).ToArray();
    private static decimal? NormalizeLatitude(decimal? v)=>v is null?null:v is >=-90m and <=90m?decimal.Round(v.Value,3):999m;
    private static decimal? NormalizeLongitude(decimal? v)=>v is null?null:v is >=-180m and <=180m?decimal.Round(v.Value,3):999m;
}

public sealed record OpportunityTimeWindow(DayOfWeek DayOfWeek,TimeOnly StartTime,TimeOnly EndTime,string TimeZoneId);
public enum ProfessionalOpportunityStatus{Draft=1,Published=2,Paused=3,Filled=4,Expired=5,Cancelled=6}
