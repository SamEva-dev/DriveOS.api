using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Proposals;

/// <summary>
/// Organization-initiated commercial proposal sent directly to a marketplace professional.
/// It remains pre-contractual: an accepted proposal is an agreement to proceed, not yet a contract,
/// workforce relationship, mission, booking or financial transaction.
/// </summary>
public sealed class ProfessionalProposal:AggregateRoot<ProfessionalProposalId>,IAuditableEntity
{
    private ProfessionalProposal(){}
    private ProfessionalProposal(
        ProfessionalProposalId id,OrganizationId organizationId,BranchId? branchId,
        ProfessionalProfileId profileId,ProfessionalOpportunityId? opportunityId,
        string subject,string message,DateOnly startsOn,DateOnly endsOn,
        string[] teachingCategoryCodes,ProfessionalEngagementType engagementType,
        ProfessionalVehicleProvisionMode vehicleProvisionMode,decimal? proposedRate,string? currency,
        ProfessionalRateUnit? rateUnit,bool negotiable,DateTimeOffset expiresAtUtc):base(id)
    {
        OrganizationId=organizationId;BranchId=branchId;ProfessionalProfileId=profileId;OpportunityId=opportunityId;
        Subject=subject.Trim();Message=message.Trim();StartsOn=startsOn;EndsOn=endsOn;TeachingCategoryCodes=Normalize(teachingCategoryCodes);
        EngagementType=engagementType;VehicleProvisionMode=vehicleProvisionMode;ProposedRate=proposedRate;
        Currency=TokenOrNull(currency);RateUnit=rateUnit;Negotiable=negotiable;ExpiresAtUtc=expiresAtUtc.ToUniversalTime();
        Status=ProfessionalProposalStatus.Sent;
    }

    public OrganizationId OrganizationId{get;private set;}
    public BranchId? BranchId{get;private set;}
    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public ProfessionalOpportunityId? OpportunityId{get;private set;}
    public string Subject{get;private set;}=string.Empty;
    public string Message{get;private set;}=string.Empty;
    public DateOnly StartsOn{get;private set;}
    public DateOnly EndsOn{get;private set;}
    public string[] TeachingCategoryCodes{get;private set;}=[];
    public ProfessionalEngagementType EngagementType{get;private set;}
    public ProfessionalVehicleProvisionMode VehicleProvisionMode{get;private set;}
    public decimal? ProposedRate{get;private set;}
    public string? Currency{get;private set;}
    public ProfessionalRateUnit? RateUnit{get;private set;}
    public bool Negotiable{get;private set;}
    public DateTimeOffset ExpiresAtUtc{get;private set;}
    public ProfessionalProposalStatus Status{get;private set;}
    public int Revision{get;private set;}=1;
    public ProfessionalProposalRevisionSnapshot[] RevisionHistory{get;private set;}=[];
    public string? DecisionReason{get;private set;}
    public DateTimeOffset SentAtUtc{get;private set;}
    public DateTimeOffset? RespondedAtUtc{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<ProfessionalProposal> Create(
        ProfessionalProposalId id,OrganizationId organizationId,BranchId? branchId,ProfessionalProfile profile,
        ProfessionalOpportunityId? opportunityId,string subject,string message,DateOnly startsOn,DateOnly endsOn,
        IEnumerable<string> teachingCategoryCodes,ProfessionalEngagementType engagementType,
        ProfessionalVehicleProvisionMode vehicleProvisionMode,decimal? proposedRate,string? currency,
        ProfessionalRateUnit? rateUnit,bool negotiable,DateTimeOffset expiresAtUtc,DateTimeOffset now,UserId actor)
    {
        var categories=Normalize(teachingCategoryCodes);
        if(id.IsEmpty||organizationId.IsEmpty||profile.Id.IsEmpty)return Result.Failure<ProfessionalProposal>(ProfessionalProposalErrors.InvalidIdentifier);
        if(!profile.IsDiscoverable)return Result.Failure<ProfessionalProposal>(ProfessionalProposalErrors.ProfileNotEligible);
        if(string.IsNullOrWhiteSpace(subject)||subject.Trim().Length>180||message?.Trim().Length>3000)
            return Result.Failure<ProfessionalProposal>(ProfessionalProposalErrors.InvalidContent);
        if(endsOn<startsOn||categories.Length==0||!categories.All(profile.TeachingCategoryCodes.Contains))
            return Result.Failure<ProfessionalProposal>(ProfessionalProposalErrors.InvalidRequirements);
        if((proposedRate is not null)&&(proposedRate<0||string.IsNullOrWhiteSpace(currency)||currency.Trim().Length!=3||rateUnit is null))
            return Result.Failure<ProfessionalProposal>(ProfessionalProposalErrors.InvalidRate);
        if(expiresAtUtc<=now)return Result.Failure<ProfessionalProposal>(ProfessionalProposalErrors.InvalidExpiration);

        var x=new ProfessionalProposal(id,organizationId,branchId,profile.Id,opportunityId,subject,message??string.Empty,startsOn,endsOn,categories,engagementType,vehicleProvisionMode,proposedRate,currency,rateUnit,negotiable,expiresAtUtc);
        x.SentAtUtc=now.ToUniversalTime();x.SetCreatedAudit(now,actor);x.AppendRevision(now,actor);return Result.Success(x);
    }

    public Result Accept(DateTimeOffset now,UserId actor)
    {
        if(IsExpired(now)){Expire(now);return Result.Failure(ProfessionalProposalErrors.ProposalExpired);}
        if(Status is not ProfessionalProposalStatus.Sent and not ProfessionalProposalStatus.Countered)
            return Result.Failure(ProfessionalProposalErrors.InvalidTransition);
        Status=ProfessionalProposalStatus.Accepted;RespondedAtUtc=now.ToUniversalTime();DecisionReason=null;SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result Reject(string? reason,DateTimeOffset now,UserId actor)
    {
        if(Status is not ProfessionalProposalStatus.Sent and not ProfessionalProposalStatus.Countered)
            return Result.Failure(ProfessionalProposalErrors.InvalidTransition);
        Status=ProfessionalProposalStatus.Rejected;DecisionReason=string.IsNullOrWhiteSpace(reason)?null:reason.Trim()[..Math.Min(reason.Trim().Length,512)];
        RespondedAtUtc=now.ToUniversalTime();SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result Counter(decimal proposedRate,string currency,ProfessionalRateUnit rateUnit,bool negotiable,string? message,DateTimeOffset now,UserId actor)
    {
        if(IsExpired(now)){Expire(now);return Result.Failure(ProfessionalProposalErrors.ProposalExpired);}
        if(Status is not ProfessionalProposalStatus.Sent and not ProfessionalProposalStatus.Countered)
            return Result.Failure(ProfessionalProposalErrors.InvalidTransition);
        if(!Negotiable||proposedRate<0||string.IsNullOrWhiteSpace(currency)||currency.Trim().Length!=3)
            return Result.Failure(ProfessionalProposalErrors.CounterNotAllowed);
        ProposedRate=decimal.Round(proposedRate,2);Currency=currency.Trim().ToUpperInvariant();RateUnit=rateUnit;
        if(!string.IsNullOrWhiteSpace(message))Message=message.Trim()[..Math.Min(message.Trim().Length,3000)];
        Revision++;Status=ProfessionalProposalStatus.Countered;RespondedAtUtc=now.ToUniversalTime();SetModifiedAudit(now,actor);AppendRevision(now,actor);return Result.Success();
    }

    public Result Withdraw(string? reason,DateTimeOffset now,UserId actor)
    {
        if(Status is ProfessionalProposalStatus.Accepted or ProfessionalProposalStatus.Rejected or ProfessionalProposalStatus.Withdrawn or ProfessionalProposalStatus.Expired)
            return Result.Failure(ProfessionalProposalErrors.InvalidTransition);
        Status=ProfessionalProposalStatus.Withdrawn;DecisionReason=string.IsNullOrWhiteSpace(reason)?null:reason.Trim()[..Math.Min(reason.Trim().Length,512)];
        RespondedAtUtc=now.ToUniversalTime();SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result Expire(DateTimeOffset now)
    {
        if(Status is ProfessionalProposalStatus.Accepted or ProfessionalProposalStatus.Rejected or ProfessionalProposalStatus.Withdrawn or ProfessionalProposalStatus.Expired)
            return Result.Failure(ProfessionalProposalErrors.InvalidTransition);
        if(!IsExpired(now))return Result.Failure(ProfessionalProposalErrors.NotYetExpired);
        Status=ProfessionalProposalStatus.Expired;RespondedAtUtc=now.ToUniversalTime();SetModifiedAudit(now,null);return Result.Success();
    }


    private void AppendRevision(DateTimeOffset now,UserId actor)
    {
        var snapshot=new ProfessionalProposalRevisionSnapshot(
            Revision,Subject,Message,StartsOn,EndsOn,TeachingCategoryCodes,EngagementType,VehicleProvisionMode,
            ProposedRate,Currency,RateUnit,Negotiable,now.ToUniversalTime(),actor);
        RevisionHistory=[..RevisionHistory,snapshot];
    }
    private bool IsExpired(DateTimeOffset now)=>now.ToUniversalTime()>=ExpiresAtUtc;
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string? TokenOrNull(string? v)=>string.IsNullOrWhiteSpace(v)?null:v.Trim().ToUpperInvariant();
    private static string[] Normalize(IEnumerable<string> v)=>v.Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x=>x.Trim().ToUpperInvariant()).Distinct(StringComparer.Ordinal).OrderBy(x=>x,StringComparer.Ordinal).ToArray();
}
public sealed record ProfessionalProposalRevisionSnapshot(
    int Revision,
    string Subject,
    string Message,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string[] TeachingCategoryCodes,
    ProfessionalEngagementType EngagementType,
    ProfessionalVehicleProvisionMode VehicleProvisionMode,
    decimal? ProposedRate,
    string? Currency,
    ProfessionalRateUnit? RateUnit,
    bool Negotiable,
    DateTimeOffset ChangedAtUtc,
    UserId ChangedByUserId);

public enum ProfessionalProposalStatus{Sent=1,Countered=2,Accepted=3,Rejected=4,Withdrawn=5,Expired=6}
