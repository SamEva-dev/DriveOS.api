using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;

/// <summary>
/// Versioned final commercial terms agreed before creating an operational professional engagement.
/// The accepted revision becomes the immutable commercial snapshot consumed by MKT-015.
/// </summary>
public sealed class ProfessionalCommercialOffer:AggregateRoot<ProfessionalCommercialOfferId>,IAuditableEntity
{
    private ProfessionalCommercialOffer(){}

    private ProfessionalCommercialOffer(
        ProfessionalCommercialOfferId id,
        OrganizationId organizationId,
        ProfessionalProfileId professionalProfileId,
        ProfessionalApplicationId? applicationId,
        ProfessionalProposalId? proposalId,
        ProfessionalOpportunityId? opportunityId,
        CommercialOfferTerms terms):base(id)
    {
        OrganizationId=organizationId;
        ProfessionalProfileId=professionalProfileId;
        ApplicationId=applicationId;
        ProposalId=proposalId;
        OpportunityId=opportunityId;
        Terms=terms;
        Revision=1;
        Status=ProfessionalCommercialOfferStatus.Draft;
    }

    public OrganizationId OrganizationId{get;private set;}
    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public ProfessionalApplicationId? ApplicationId{get;private set;}
    public ProfessionalProposalId? ProposalId{get;private set;}
    public ProfessionalOpportunityId? OpportunityId{get;private set;}
    public CommercialOfferTerms Terms{get;private set;}=default!;
    public int Revision{get;private set;}
    public ProfessionalCommercialOfferRevisionSnapshot[] RevisionHistory{get;private set;}=[];
    public ProfessionalCommercialOfferStatus Status{get;private set;}
    public DateTimeOffset? SentAtUtc{get;private set;}
    public DateTimeOffset? OrganizationAcceptedAtUtc{get;private set;}
    public DateTimeOffset? ProfessionalAcceptedAtUtc{get;private set;}
    public DateTimeOffset? FinalizedAtUtc{get;private set;}
    public UserId? OrganizationAcceptedByUserId{get;private set;}
    public UserId? ProfessionalAcceptedByUserId{get;private set;}
    public string? CancellationReason{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<ProfessionalCommercialOffer> Create(
        ProfessionalCommercialOfferId id,
        OrganizationId organizationId,
        ProfessionalProfileId profileId,
        ProfessionalApplicationId? applicationId,
        ProfessionalProposalId? proposalId,
        ProfessionalOpportunityId? opportunityId,
        CommercialOfferTerms terms,
        DateTimeOffset now,
        UserId actor)
    {
        if(id.IsEmpty||organizationId.IsEmpty||profileId.IsEmpty)
            return Result.Failure<ProfessionalCommercialOffer>(ProfessionalCommercialOfferErrors.InvalidIdentifier);
        if((applicationId is null)==(proposalId is null))
            return Result.Failure<ProfessionalCommercialOffer>(ProfessionalCommercialOfferErrors.InvalidSource);
        var validation=terms.Validate();
        if(validation.IsFailure)return Result.Failure<ProfessionalCommercialOffer>(validation.Error);

        var x=new ProfessionalCommercialOffer(id,organizationId,profileId,applicationId,proposalId,opportunityId,terms.Normalize());
        x.SetCreatedAudit(now,actor);
        return Result.Success(x);
    }

    public Result Revise(CommercialOfferTerms terms,DateTimeOffset now,UserId actor)
    {
        if(Status is ProfessionalCommercialOfferStatus.Finalized or ProfessionalCommercialOfferStatus.Cancelled)
            return Result.Failure(ProfessionalCommercialOfferErrors.InvalidTransition);
        var validation=terms.Validate();if(validation.IsFailure)return validation;
        CaptureCurrentRevision(now,actor);
        Terms=terms.Normalize();
        Revision++;
        Status=ProfessionalCommercialOfferStatus.Draft;
        SentAtUtc=null;
        OrganizationAcceptedAtUtc=null;
        ProfessionalAcceptedAtUtc=null;
        OrganizationAcceptedByUserId=null;
        ProfessionalAcceptedByUserId=null;
        FinalizedAtUtc=null;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result Send(DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalCommercialOfferStatus.Draft)
            return Result.Failure(ProfessionalCommercialOfferErrors.InvalidTransition);
        Status=ProfessionalCommercialOfferStatus.Sent;
        SentAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result AcceptByOrganization(DateTimeOffset now,UserId actor)
    {
        if(Status is not ProfessionalCommercialOfferStatus.Sent and not ProfessionalCommercialOfferStatus.PartiallyAccepted)
            return Result.Failure(ProfessionalCommercialOfferErrors.InvalidTransition);
        OrganizationAcceptedAtUtc=now.ToUniversalTime();
        OrganizationAcceptedByUserId=actor;
        return UpdateAcceptanceStatus(now,actor);
    }

    public Result AcceptByProfessional(DateTimeOffset now,UserId actor)
    {
        if(Status is not ProfessionalCommercialOfferStatus.Sent and not ProfessionalCommercialOfferStatus.PartiallyAccepted)
            return Result.Failure(ProfessionalCommercialOfferErrors.InvalidTransition);
        ProfessionalAcceptedAtUtc=now.ToUniversalTime();
        ProfessionalAcceptedByUserId=actor;
        return UpdateAcceptanceStatus(now,actor);
    }

    public Result FinalizeOffer(DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalCommercialOfferStatus.Accepted)
            return Result.Failure(ProfessionalCommercialOfferErrors.BilateralAcceptanceRequired);
        Status=ProfessionalCommercialOfferStatus.Finalized;
        FinalizedAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result Cancel(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status is ProfessionalCommercialOfferStatus.Finalized or ProfessionalCommercialOfferStatus.Cancelled)
            return Result.Failure(ProfessionalCommercialOfferErrors.InvalidTransition);
        reason=(reason??"").Trim();
        if(reason.Length is <2 or >512)return Result.Failure(ProfessionalCommercialOfferErrors.CancellationReasonRequired);
        Status=ProfessionalCommercialOfferStatus.Cancelled;
        CancellationReason=reason;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }


    private void CaptureCurrentRevision(DateTimeOffset now,UserId actor)
    {
        var snapshot=new ProfessionalCommercialOfferRevisionSnapshot(
            Revision,
            Terms,
            now.ToUniversalTime(),
            actor);
        RevisionHistory=[..RevisionHistory,snapshot];
    }

    private Result UpdateAcceptanceStatus(DateTimeOffset now,UserId actor)
    {
        Status=OrganizationAcceptedAtUtc is not null&&ProfessionalAcceptedAtUtc is not null
            ?ProfessionalCommercialOfferStatus.Accepted
            :ProfessionalCommercialOfferStatus.PartiallyAccepted;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
}

public sealed record CommercialOfferTerms(
    DateOnly StartsOn,
    DateOnly EndsOn,
    string[] TeachingCategoryCodes,
    ProfessionalEngagementType EngagementType,
    ProfessionalVehicleProvisionMode VehicleProvisionMode,
    int? EstimatedMinutes,
    decimal? RateAmount,
    string? Currency,
    ProfessionalRateUnit? RateUnit,
    decimal? MileageRate,
    decimal? VehicleAllowance,
    decimal? MinimumGuaranteedAmount,
    string[] ClauseCodes)
{
    public Result Validate()
    {
        if(EndsOn<StartsOn||TeachingCategoryCodes is null||TeachingCategoryCodes.Length==0)
            return Result.Failure(ProfessionalCommercialOfferErrors.InvalidTerms);
        if(EstimatedMinutes is <=0 or >100000)return Result.Failure(ProfessionalCommercialOfferErrors.InvalidTerms);
        if(RateAmount is not null&&(RateAmount<0||string.IsNullOrWhiteSpace(Currency)||Currency.Trim().Length!=3||RateUnit is null))
            return Result.Failure(ProfessionalCommercialOfferErrors.InvalidTerms);
        if(MileageRate<0||VehicleAllowance<0||MinimumGuaranteedAmount<0)
            return Result.Failure(ProfessionalCommercialOfferErrors.InvalidTerms);
        return Result.Success();
    }

    public CommercialOfferTerms Normalize()=>this with
    {
        TeachingCategoryCodes=TeachingCategoryCodes.Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x=>x.Trim().ToUpperInvariant()).Distinct(StringComparer.Ordinal).OrderBy(x=>x,StringComparer.Ordinal).ToArray(),
        Currency=string.IsNullOrWhiteSpace(Currency)?null:Currency.Trim().ToUpperInvariant(),
        RateAmount=RateAmount is null?null:decimal.Round(RateAmount.Value,2),
        MileageRate=MileageRate is null?null:decimal.Round(MileageRate.Value,3),
        VehicleAllowance=VehicleAllowance is null?null:decimal.Round(VehicleAllowance.Value,2),
        MinimumGuaranteedAmount=MinimumGuaranteedAmount is null?null:decimal.Round(MinimumGuaranteedAmount.Value,2),
        ClauseCodes=(ClauseCodes??[]).Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x=>x.Trim().ToUpperInvariant()).Distinct(StringComparer.Ordinal).OrderBy(x=>x,StringComparer.Ordinal).ToArray()
    };
}

public enum ProfessionalCommercialOfferStatus{Draft=1,Sent=2,PartiallyAccepted=3,Accepted=4,Finalized=5,Cancelled=6}


public sealed record ProfessionalCommercialOfferRevisionSnapshot(
    int Revision,
    CommercialOfferTerms Terms,
    DateTimeOffset ChangedAtUtc,
    UserId ChangedByUserId);
