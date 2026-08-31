using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;

/// <summary>
/// Versioned operational reaction attached to one compliance requirement in one jurisdiction.
/// The requirement says what evidence is mandatory; this policy says what DriveOS must do when it becomes invalid.
/// </summary>
public sealed class ProfessionalComplianceCriticalityPolicy
    :AggregateRoot<ProfessionalCompliancePolicyId>,IAuditableEntity
{
    private ProfessionalComplianceCriticalityPolicy(){}

    private ProfessionalComplianceCriticalityPolicy(
        ProfessionalCompliancePolicyId id,
        string countryCode,
        string requirementCode,
        ProfessionalComplianceCriticality criticality,
        ProfessionalComplianceEnforcementAction action,
        int gracePeriodDays,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        int version):base(id)
    {
        CountryCode=Token(countryCode);
        RequirementCode=Token(requirementCode);
        Criticality=criticality;
        Action=action;
        GracePeriodDays=gracePeriodDays;
        EffectiveFrom=effectiveFrom;
        EffectiveTo=effectiveTo;
        Version=version;
        Status=ProfessionalCompliancePolicyStatus.Active;
    }

    public string CountryCode{get;private set;}=string.Empty;
    public string RequirementCode{get;private set;}=string.Empty;
    public ProfessionalComplianceCriticality Criticality{get;private set;}
    public ProfessionalComplianceEnforcementAction Action{get;private set;}
    public int GracePeriodDays{get;private set;}
    public DateOnly EffectiveFrom{get;private set;}
    public DateOnly? EffectiveTo{get;private set;}
    public int Version{get;private set;}
    public ProfessionalCompliancePolicyStatus Status{get;private set;}

    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public bool AppliesOn(DateOnly date)=>
        Status==ProfessionalCompliancePolicyStatus.Active&&
        EffectiveFrom<=date&&
        (EffectiveTo is null||date<=EffectiveTo.Value);

    public static Result<ProfessionalComplianceCriticalityPolicy> Create(
        ProfessionalCompliancePolicyId id,
        string countryCode,
        string requirementCode,
        ProfessionalComplianceCriticality criticality,
        ProfessionalComplianceEnforcementAction action,
        int gracePeriodDays,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        int version,
        DateTimeOffset now,
        UserId actor)
    {
        if(id.IsEmpty||countryCode?.Trim().Length!=2||string.IsNullOrWhiteSpace(requirementCode)||
           version<1||gracePeriodDays is <0 or >365)
            return Result.Failure<ProfessionalComplianceCriticalityPolicy>(
                ProfessionalCompliancePolicyErrors.InvalidPolicy);

        if(effectiveTo is DateOnly to&&to<effectiveFrom)
            return Result.Failure<ProfessionalComplianceCriticalityPolicy>(
                ProfessionalCompliancePolicyErrors.InvalidPeriod);

        var x=new ProfessionalComplianceCriticalityPolicy(
            id,countryCode,requirementCode,criticality,action,gracePeriodDays,effectiveFrom,effectiveTo,version);
        x.SetCreatedAudit(now,actor);
        return Result.Success(x);
    }

    public Result Retire(DateTimeOffset now,UserId actor)
    {
        if(Status==ProfessionalCompliancePolicyStatus.Retired)
            return Result.Failure(ProfessionalCompliancePolicyErrors.InvalidTransition);

        Status=ProfessionalCompliancePolicyStatus.Retired;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string Token(string? value)=>(value??string.Empty).Trim().ToUpperInvariant();
}

public enum ProfessionalComplianceCriticality{Low=1,Medium=2,High=3,Critical=4}
public enum ProfessionalComplianceEnforcementAction{AlertOnly=1,BlockNewSessions=2,PauseMissions=3,SuspendProfessional=4}
public enum ProfessionalCompliancePolicyStatus{Active=1,Retired=2}
