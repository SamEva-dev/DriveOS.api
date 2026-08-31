using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;

/// <summary>
/// Versionable requirement used to evaluate whether a professional may be represented as compliant
/// in the marketplace. Requirements are scoped by jurisdiction, professional type and optional
/// teaching categories. They describe required evidence; they do not own uploaded documents.
/// </summary>
public sealed class ProfessionalComplianceRequirement : AggregateRoot<ProfessionalComplianceRequirementId>, IAuditableEntity
{
    private ProfessionalComplianceRequirement() { }

    private ProfessionalComplianceRequirement(
        ProfessionalComplianceRequirementId id,
        string requirementCode,
        string countryCode,
        ProfessionalType professionalType,
        ProfessionalEvidenceKind evidenceKind,
        string evidenceTypeCode,
        bool mandatory,
        bool blocking,
        string[] applicableCategoryCodes,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        int version) : base(id)
    {
        RequirementCode=Token(requirementCode); CountryCode=Token(countryCode); ProfessionalType=professionalType;
        EvidenceKind=evidenceKind; EvidenceTypeCode=Token(evidenceTypeCode); Mandatory=mandatory; Blocking=blocking;
        ApplicableCategoryCodes=Normalize(applicableCategoryCodes); EffectiveFrom=effectiveFrom; EffectiveTo=effectiveTo;
        Version=version; Status=ProfessionalComplianceRequirementStatus.Active;
    }

    public string RequirementCode{get;private set;}=string.Empty;
    public string CountryCode{get;private set;}=string.Empty;
    public ProfessionalType ProfessionalType{get;private set;}
    public ProfessionalEvidenceKind EvidenceKind{get;private set;}
    public string EvidenceTypeCode{get;private set;}=string.Empty;
    public bool Mandatory{get;private set;}
    public bool Blocking{get;private set;}
    public string[] ApplicableCategoryCodes{get;private set;}=[];
    public DateOnly EffectiveFrom{get;private set;}
    public DateOnly? EffectiveTo{get;private set;}
    public int Version{get;private set;}
    public ProfessionalComplianceRequirementStatus Status{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public bool AppliesOn(DateOnly date,IReadOnlyCollection<string> categories)=>
        Status==ProfessionalComplianceRequirementStatus.Active &&
        EffectiveFrom<=date && (EffectiveTo is null||date<=EffectiveTo.Value) &&
        (ApplicableCategoryCodes.Length==0||ApplicableCategoryCodes.Any(categories.Contains));

    public static Result<ProfessionalComplianceRequirement> Create(
        ProfessionalComplianceRequirementId id,string requirementCode,string countryCode,
        ProfessionalType professionalType,ProfessionalEvidenceKind evidenceKind,string evidenceTypeCode,
        bool mandatory,bool blocking,IEnumerable<string>? applicableCategoryCodes,
        DateOnly effectiveFrom,DateOnly? effectiveTo,int version,DateTimeOffset now,UserId actor)
    {
        if(id.IsEmpty||string.IsNullOrWhiteSpace(requirementCode)||countryCode?.Trim().Length!=2||
           string.IsNullOrWhiteSpace(evidenceTypeCode)||version<1)
            return Result.Failure<ProfessionalComplianceRequirement>(ProfessionalComplianceErrors.InvalidRequirement);
        if(effectiveTo is DateOnly to&&to<effectiveFrom)
            return Result.Failure<ProfessionalComplianceRequirement>(ProfessionalComplianceErrors.InvalidValidityPeriod);
        var x=new ProfessionalComplianceRequirement(id,requirementCode,countryCode,professionalType,evidenceKind,evidenceTypeCode,mandatory,blocking,(applicableCategoryCodes??[]).ToArray(),effectiveFrom,effectiveTo,version);
        x.SetCreatedAudit(now,actor); return Result.Success(x);
    }

    public Result Retire(DateTimeOffset now,UserId actor)
    {
        if(Status==ProfessionalComplianceRequirementStatus.Retired)return Result.Failure(ProfessionalComplianceErrors.InvalidRequirementTransition);
        Status=ProfessionalComplianceRequirementStatus.Retired; SetModifiedAudit(now,actor); return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string Token(string? v)=>(v??"").Trim().ToUpperInvariant();
    private static string[] Normalize(IEnumerable<string> v)=>v.Where(x=>!string.IsNullOrWhiteSpace(x)).Select(Token).Distinct(StringComparer.Ordinal).OrderBy(x=>x,StringComparer.Ordinal).ToArray();
}
public enum ProfessionalEvidenceKind{Document=1,Credential=2}
public enum ProfessionalComplianceRequirementStatus{Active=1,Retired=2}
