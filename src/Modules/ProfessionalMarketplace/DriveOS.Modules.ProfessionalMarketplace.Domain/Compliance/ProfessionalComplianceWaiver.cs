using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;

/// <summary>
/// Audited temporary exception to one compliance requirement for one professional profile.
/// It never changes the evidence itself and expires automatically at ValidUntil.
/// </summary>
public sealed class ProfessionalComplianceWaiver
    :AggregateRoot<ProfessionalComplianceWaiverId>,IAuditableEntity
{
    private ProfessionalComplianceWaiver(){}

    private ProfessionalComplianceWaiver(
        ProfessionalComplianceWaiverId id,
        ProfessionalProfileId profileId,
        string requirementCode,
        string countryCode,
        DateOnly validFrom,
        DateOnly validUntil,
        string reason,
        UserId approvedBy):base(id)
    {
        ProfessionalProfileId=profileId;
        RequirementCode=Token(requirementCode);
        CountryCode=Token(countryCode);
        ValidFrom=validFrom;
        ValidUntil=validUntil;
        Reason=reason.Trim();
        ApprovedByUserId=approvedBy;
        Status=ProfessionalComplianceWaiverStatus.Active;
    }

    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public string RequirementCode{get;private set;}=string.Empty;
    public string CountryCode{get;private set;}=string.Empty;
    public DateOnly ValidFrom{get;private set;}
    public DateOnly ValidUntil{get;private set;}
    public string Reason{get;private set;}=string.Empty;
    public UserId ApprovedByUserId{get;private set;}
    public ProfessionalComplianceWaiverStatus Status{get;private set;}
    public DateTimeOffset? RevokedAtUtc{get;private set;}
    public UserId? RevokedByUserId{get;private set;}
    public string? RevocationReason{get;private set;}

    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public bool IsEffectiveOn(DateOnly date)=>
        Status==ProfessionalComplianceWaiverStatus.Active&&ValidFrom<=date&&date<=ValidUntil;

    public static Result<ProfessionalComplianceWaiver> Create(
        ProfessionalComplianceWaiverId id,
        ProfessionalProfileId profileId,
        string requirementCode,
        string countryCode,
        DateOnly validFrom,
        DateOnly validUntil,
        string reason,
        DateTimeOffset now,
        UserId actor)
    {
        reason=(reason??string.Empty).Trim();
        if(id.IsEmpty||profileId.IsEmpty||countryCode?.Trim().Length!=2||
           string.IsNullOrWhiteSpace(requirementCode)||reason.Length is <5 or >1000)
            return Result.Failure<ProfessionalComplianceWaiver>(ProfessionalComplianceWaiverErrors.InvalidWaiver);

        if(validUntil<validFrom)
            return Result.Failure<ProfessionalComplianceWaiver>(ProfessionalComplianceWaiverErrors.InvalidPeriod);

        var x=new ProfessionalComplianceWaiver(
            id,profileId,requirementCode,countryCode,validFrom,validUntil,reason,actor);
        x.SetCreatedAudit(now,actor);
        return Result.Success(x);
    }

    public Result Revoke(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalComplianceWaiverStatus.Active)
            return Result.Failure(ProfessionalComplianceWaiverErrors.InvalidTransition);

        reason=(reason??string.Empty).Trim();
        if(reason.Length is <2 or >512)
            return Result.Failure(ProfessionalComplianceWaiverErrors.ReasonRequired);

        Status=ProfessionalComplianceWaiverStatus.Revoked;
        RevokedAtUtc=now.ToUniversalTime();
        RevokedByUserId=actor;
        RevocationReason=reason;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result Expire(DateOnly today,DateTimeOffset now)
    {
        if(Status!=ProfessionalComplianceWaiverStatus.Active)
            return Result.Failure(ProfessionalComplianceWaiverErrors.InvalidTransition);
        if(today<=ValidUntil)
            return Result.Failure(ProfessionalComplianceWaiverErrors.NotYetExpired);

        Status=ProfessionalComplianceWaiverStatus.Expired;
        SetModifiedAudit(now,null);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string Token(string? value)=>(value??string.Empty).Trim().ToUpperInvariant();
}

public enum ProfessionalComplianceWaiverStatus{Active=1,Revoked=2,Expired=3}
