using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Applications;

/// <summary>
/// Professional-initiated response to a published opportunity.
/// It records candidate intent and proposed commercial terms only; acceptance does not itself create
/// a contract, workforce relationship, mission or scheduling booking.
/// </summary>
public sealed class ProfessionalApplication:AggregateRoot<ProfessionalApplicationId>,IAuditableEntity
{
    private ProfessionalApplication(){}
    private ProfessionalApplication(ProfessionalApplicationId id,ProfessionalOpportunityId opportunityId,ProfessionalProfileId profileId,OrganizationId organizationId,string message,decimal? proposedRate,string? currency,ProfessionalRateUnit? unit,bool negotiable,DateOnly? availableFrom,DateOnly? availableUntil):base(id)
    {
        OpportunityId=opportunityId;ProfessionalProfileId=profileId;OrganizationId=organizationId;Message=message.Trim();
        ProposedRate=proposedRate;Currency=TokenOrNull(currency);RateUnit=unit;Negotiable=negotiable;
        AvailableFrom=availableFrom;AvailableUntil=availableUntil;Status=ProfessionalApplicationStatus.Submitted;
    }
    public ProfessionalOpportunityId OpportunityId{get;private set;}
    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public OrganizationId OrganizationId{get;private set;}
    public string Message{get;private set;}=string.Empty;
    public decimal? ProposedRate{get;private set;}
    public string? Currency{get;private set;}
    public ProfessionalRateUnit? RateUnit{get;private set;}
    public bool Negotiable{get;private set;}
    public DateOnly? AvailableFrom{get;private set;}
    public DateOnly? AvailableUntil{get;private set;}
    public ProfessionalApplicationStatus Status{get;private set;}
    public string? DecisionReason{get;private set;}
    public DateTimeOffset SubmittedAtUtc{get;private set;}
    public DateTimeOffset? DecidedAtUtc{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<ProfessionalApplication> Create(ProfessionalApplicationId id,ProfessionalOpportunity opportunity,ProfessionalProfile profile,string message,decimal? proposedRate,string? currency,ProfessionalRateUnit? unit,bool negotiable,DateOnly? availableFrom,DateOnly? availableUntil,DateTimeOffset now,UserId actor)
    {
        if(id.IsEmpty||opportunity.Id.IsEmpty||profile.Id.IsEmpty)return Result.Failure<ProfessionalApplication>(ProfessionalApplicationErrors.InvalidIdentifier);
        if(opportunity.Status!=ProfessionalOpportunityStatus.Published)return Result.Failure<ProfessionalApplication>(ProfessionalApplicationErrors.OpportunityNotOpen);
        if(!profile.IsDiscoverable)return Result.Failure<ProfessionalApplication>(ProfessionalApplicationErrors.ProfileNotEligible);
        if(!opportunity.TeachingCategoryCodes.Any(profile.TeachingCategoryCodes.Contains))return Result.Failure<ProfessionalApplication>(ProfessionalApplicationErrors.ProfileNotEligible);
        message=(message??"").Trim();if(message.Length>2000)return Result.Failure<ProfessionalApplication>(ProfessionalApplicationErrors.InvalidMessage);
        if((proposedRate is not null)&&(proposedRate<0||string.IsNullOrWhiteSpace(currency)||currency.Trim().Length!=3||unit is null))
            return Result.Failure<ProfessionalApplication>(ProfessionalApplicationErrors.InvalidRate);
        if(availableUntil is DateOnly end&&availableFrom is DateOnly start&&end<start)
            return Result.Failure<ProfessionalApplication>(ProfessionalApplicationErrors.InvalidAvailability);

        var x=new ProfessionalApplication(id,opportunity.Id,profile.Id,opportunity.OrganizationId,message,proposedRate,currency,unit,negotiable,availableFrom,availableUntil);
        x.SubmittedAtUtc=now.ToUniversalTime();x.SetCreatedAudit(now,actor);return Result.Success(x);
    }

    public Result StartReview(DateTimeOffset now,UserId actor)=>MoveFrom(ProfessionalApplicationStatus.Submitted,ProfessionalApplicationStatus.UnderReview,now,actor);
    public Result Shortlist(DateTimeOffset now,UserId actor)
    {
        if(Status is not ProfessionalApplicationStatus.Submitted and not ProfessionalApplicationStatus.UnderReview)return Result.Failure(ProfessionalApplicationErrors.InvalidTransition);
        Status=ProfessionalApplicationStatus.Shortlisted;SetModifiedAudit(now,actor);return Result.Success();
    }
    public Result Accept(DateTimeOffset now,UserId actor)
    {
        if(Status is not ProfessionalApplicationStatus.Submitted and not ProfessionalApplicationStatus.UnderReview and not ProfessionalApplicationStatus.Shortlisted)return Result.Failure(ProfessionalApplicationErrors.InvalidTransition);
        Status=ProfessionalApplicationStatus.Accepted;DecidedAtUtc=now.ToUniversalTime();DecisionReason=null;SetModifiedAudit(now,actor);return Result.Success();
    }
    public Result Reject(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status is ProfessionalApplicationStatus.Accepted or ProfessionalApplicationStatus.Rejected or ProfessionalApplicationStatus.Withdrawn)return Result.Failure(ProfessionalApplicationErrors.InvalidTransition);
        reason=(reason??"").Trim();if(reason.Length is <2 or >512)return Result.Failure(ProfessionalApplicationErrors.DecisionReasonRequired);
        Status=ProfessionalApplicationStatus.Rejected;DecisionReason=reason;DecidedAtUtc=now.ToUniversalTime();SetModifiedAudit(now,actor);return Result.Success();
    }
    public Result Withdraw(string? reason,DateTimeOffset now,UserId actor)
    {
        if(Status is ProfessionalApplicationStatus.Accepted or ProfessionalApplicationStatus.Rejected or ProfessionalApplicationStatus.Withdrawn)return Result.Failure(ProfessionalApplicationErrors.InvalidTransition);
        Status=ProfessionalApplicationStatus.Withdrawn;DecisionReason=string.IsNullOrWhiteSpace(reason)?null:reason.Trim()[..Math.Min(reason.Trim().Length,512)];DecidedAtUtc=now.ToUniversalTime();SetModifiedAudit(now,actor);return Result.Success();
    }
    private Result MoveFrom(ProfessionalApplicationStatus from,ProfessionalApplicationStatus to,DateTimeOffset now,UserId actor){if(Status!=from)return Result.Failure(ProfessionalApplicationErrors.InvalidTransition);Status=to;SetModifiedAudit(now,actor);return Result.Success();}
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string? TokenOrNull(string? v)=>string.IsNullOrWhiteSpace(v)?null:v.Trim().ToUpperInvariant();
}
public enum ProfessionalApplicationStatus{Submitted=1,UnderReview=2,Shortlisted=3,Accepted=4,Rejected=5,Withdrawn=6}
